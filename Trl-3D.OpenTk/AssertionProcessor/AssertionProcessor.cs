using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Trl_3D.Core.Abstractions;
using Trl_3D.Core.Assertions;
using Trl_3D.Core.Scene;
using Trl_3D.OpenTk.GeometryBuffers;

namespace Trl_3D.OpenTk.AssertionProcessor
{
    public class AssertionProcessor : IAssertionProcessor { 

        private readonly ILogger<AssertionProcessor> _logger;

        private readonly IRenderWindow _renderWindow;
        private readonly CancellationTokenSource _cancellationTokenManager;
        private readonly IImageLoader _imageLoader;
        private readonly SceneGraph _sceneGraph;

        private readonly LinkedList<Core.Scene.Triangle> _partialObjectsWatchList; // keep track of partial objects that may become complete in a future batch

        private readonly BufferManager _bufferManager;

        public Channel<AssertionBatch> AssertionUpdatesChannel { get; private set; }

        public AssertionProcessor(IImageLoader imageLoader,
                                  ILogger<AssertionProcessor> logger,
                                  IRenderWindow renderWindow,
                                  CancellationTokenSource cancellationTokenManager,
                                  SceneGraph sceneGraph,
                                  BufferManager bufferManager)
        {
            _logger = logger;
            _renderWindow = renderWindow;
            _cancellationTokenManager = cancellationTokenManager;
            _logger.LogInformation("Scene created.");
            _imageLoader = imageLoader;
            _sceneGraph = sceneGraph;

            AssertionUpdatesChannel = Channel.CreateUnbounded<AssertionBatch>();

            _partialObjectsWatchList = new LinkedList<Core.Scene.Triangle>();

            _bufferManager = bufferManager;
        }

        public async Task StartAssertionConsumer()
        {
            await foreach (var assertionBatch in AssertionUpdatesChannel.Reader.ReadAllAsync(_cancellationTokenManager.Token)
                .WithCancellation(_cancellationTokenManager.Token))
            {
                if (assertionBatch.Assertions == null || !assertionBatch.Assertions.Any())
                {
                    _logger.LogWarning("Assertion batch has no assertions");
                    continue;
                }

                // Scene graph is updated per batch to group updates together and manage vertex buffers
                try
                {
                    await foreach (var renderCommand in Process(assertionBatch)
                        .WithCancellation(_cancellationTokenManager.Token))
                    {
                        await _renderWindow.RenderCommandUpdatesChannel.Writer.WriteAsync(renderCommand, 
                            _cancellationTokenManager.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Scene assertion processor failed");
                }
            }

            _logger.LogInformation("Scene assertion consumer stopped.");
        }

        /// <summary>
        /// Generates scene graph updates for renderer.
        /// </summary>
        public async IAsyncEnumerable<IRenderCommand> Process(AssertionBatch assertionBatch)
        {
            var knownUpdateObjects = new List<ObjectIdentityBase>();

            foreach (var assertion in assertionBatch.Assertions)
            {
                if (assertion is GetPickingInfo getPickingInfo)
                {
                    yield return new RenderCommands.RequestPickingInfo(getPickingInfo.ScreenX, getPickingInfo.ScreenY);
                }
                else if (assertion is Core.Assertions.ClearColor clearColor)
                {
                    _sceneGraph.RgbClearColor = new (clearColor.Red, clearColor.Green, clearColor.Blue, 1.0f);
                    yield return new RenderCommands.ClearColor(_sceneGraph.RgbClearColor);
                }
                else if (assertion is CameraProjectionPerspective projectionPerspective)
                {
                    // Projection matrix is set in uniforms
                    yield return new RenderCommands.SetProjectionMatrix(_sceneGraph, projectionPerspective.FieldOfViewVerticalDegrees, 
                        projectionPerspective.NearPlane, projectionPerspective.FarPlane);
                }
                else if (assertion is CameraOrientation cameraOrientation)
                {
                    // View matrix is set in uniforms
                    _sceneGraph.ViewMatrix = cameraOrientation.ToMatrix();
                }
                else if (assertion is Core.Assertions.Vertex assertionVertex)
                {
                    if (!_sceneGraph.Vertices.TryGetValue(assertionVertex.VertexId, out Core.Scene.Vertex vertex))
                    {
                        vertex = new Core.Scene.Vertex(_sceneGraph, assertionVertex.VertexId);
                        _sceneGraph.Vertices[assertionVertex.VertexId] = vertex;
                    }
                    if (assertionVertex.Coordinates != default)
                    {
                        vertex.Coordinates = assertionVertex.Coordinates;
                        knownUpdateObjects.Add(vertex);
                    }
                }
                else if (assertion is Core.Assertions.Triangle triangle)
                {
                    var newTriangle = new Core.Scene.Triangle(_sceneGraph, triangle.TriangleId) { VertexIds = triangle.VertexIds };
                    _partialObjectsWatchList.AddLast(newTriangle);
                    _sceneGraph.Triangles[triangle.TriangleId] = newTriangle;
                }
                else if (assertion is Core.Assertions.GrabScreenshot)
                {
                    yield return new RenderCommands.GrabScreenshot(_renderWindow, _cancellationTokenManager);
                }
                else if (assertion is Core.Assertions.Texture texture)
                {
                    // Pre-load texture to avoid doing this in the function setting OpenGL state
                    var loadedImage = await _imageLoader.LoadImage(new Uri(texture.Uri));
                    _sceneGraph.Textures[texture.TextureId] = new Core.Scene.Texture(_sceneGraph, texture.TextureId, loadedImage.BufferRgba,
                        loadedImage.Width, loadedImage.Height);
                }
                else if (assertion is TexCoords texCoords)
                {
                    _sceneGraph.SurfaceVertexTexCoords[texCoords.ObjectIdentifier] = texCoords;
                }
                else if (assertion is SurfaceColor surfaceColor)
                {
                    _sceneGraph.SurfaceVertexColors[surfaceColor.ObjectIdentifier] = surfaceColor;
                }
                else
                {
                    throw new ArgumentException($"Unknown assertion type: {assertion.GetType()}");
                }
            }

            // Split objects into new and update lists
            var newRenderTriangles = new List<Core.Scene.Triangle>();

            // Get known complete geometry
            LinkedListNode<Core.Scene.Triangle> currentNode = _partialObjectsWatchList.First;
            while (currentNode != null)
            {
                if (_bufferManager.HasExistingTriangleBuffer(currentNode.Value))
                {
                    knownUpdateObjects.Add(currentNode.Value);
                    currentNode = currentNode.Next;
                    continue;
                }
                
                if (!currentNode.Value.HasMinimumRenderInfo)
                {
                    currentNode = currentNode.Next;
                    continue;
                }

                newRenderTriangles.Add(currentNode.Value);
                var next = currentNode.Next;
                _partialObjectsWatchList.Remove(currentNode);
                currentNode = next;
            }

            if (newRenderTriangles.Any())
            {
                yield return _bufferManager.CreateRenderCommands(newRenderTriangles);                
            }

            if (knownUpdateObjects.Any())
            {
                foreach (var reloadCommand in _bufferManager.CreateReloadCommands(knownUpdateObjects))
                {
                    yield return reloadCommand;
                }
            }
        }
    }
}
