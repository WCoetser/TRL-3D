using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Trl_3D.Core.Abstractions;
using Trl_3D.Core.Assertions;
using Trl_3D.Core.Scene;
using Trl_3D.OpenTk.GeometryBuffers;
using Trl_3D.OpenTk.RenderCommands;

namespace Trl_3D.OpenTk.AssertionProcessor
{
    public class AssertionProcessor : IAssertionProcessor { 

        private readonly ILogger<AssertionProcessor> _logger;

        private readonly IRenderWindow _renderWindow;
        private readonly ICancellationTokenManager _cancellationTokenManager;
        private readonly IImageLoader _imageLoader;
        private readonly SceneGraph _sceneGraph;

        private readonly LinkedList<Core.Scene.Triangle> _partialObjectsWatchList; // keep track of partial objects that may become complete in a future batch

        private readonly BufferManager _bufferManager;

        public Channel<AssertionBatch> AssertionUpdatesChannel { get; private set; }

        public AssertionProcessor(IImageLoader imageLoader,
                                  ILogger<AssertionProcessor> logger,
                                  IRenderWindow renderWindow,
                                  ICancellationTokenManager cancellationTokenManager,
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
            await foreach (var assertionBatch in AssertionUpdatesChannel.Reader.ReadAllAsync(_cancellationTokenManager.CancellationToken)
                .WithCancellation(_cancellationTokenManager.CancellationToken))
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
                        .WithCancellation(_cancellationTokenManager.CancellationToken))
                    {
                        await _renderWindow.RenderCommandUpdatesChannel.Writer.WriteAsync(renderCommand, 
                            _cancellationTokenManager.CancellationToken);
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
                    _sceneGraph.SurfaceVertexColors[surfaceColor.ObjectIdentifier] = surfaceColor.vertexColor;
                }
                else
                {
                    throw new ArgumentException($"Unknown assertion type: {assertion.GetType()}");
                }
            }

            // Get known complete geometry
            var renderTriangles = new List<Core.Scene.Triangle>();
            LinkedListNode<Core.Scene.Triangle> currentNode = _partialObjectsWatchList.First;
            while (currentNode != null)
            {
                if (!currentNode.Value.HasMinimumRenderInfo)
                {
                    currentNode = currentNode.Next;
                }
                else
                {
                    renderTriangles.Add(currentNode.Value);
                    var next = currentNode.Next;
                    _partialObjectsWatchList.Remove(currentNode);
                    currentNode = next;
                }
            }

            if (renderTriangles.Any())
            {
                yield return _bufferManager.CreateRenderCommands(renderTriangles);                
            }

            //// Test 
            foreach (var reloadCommand in _bufferManager.CreateReloadCommands())
            {
                yield return reloadCommand;
            }            
        }
    }
}
