using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using System;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;
using Trl_3D.Core.Abstractions;
using Trl_3D.Core.Assertions;
using Trl_3D.Core.Scene;
using Trl_3D.Core.Scene.Updates;

namespace Trl_3D.OpenTk.AssertionProcessor
{
    public class AssertionProcessor : IAssertionProcessor { 

        private readonly ILogger<AssertionProcessor> _logger;

        private readonly IRenderWindow _renderWindow;
        private readonly ICancellationTokenManager _cancellationTokenManager;
        private readonly IImageLoader _imageLoader;

        public Channel<AssertionBatch> AssertionUpdatesChannel { get; private set; }

        public AssertionProcessor(IImageLoader imageLoader,
                                  ILogger<AssertionProcessor> logger,
                                  IRenderWindow renderWindow,
                                  ICancellationTokenManager cancellationTokenManager)
        {
            _logger = logger;
            _renderWindow = renderWindow;
            _cancellationTokenManager = cancellationTokenManager;
            _logger.LogInformation("Scene created.");
            _imageLoader = imageLoader;

            AssertionUpdatesChannel = Channel.CreateUnbounded<AssertionBatch>();
        }

        public async Task StartAssertionConsumer()
        {
            await foreach (var assertionBatch in AssertionUpdatesChannel.Reader.ReadAllAsync(_cancellationTokenManager.CancellationToken))
            {
                if (assertionBatch.Assertions == null || !assertionBatch.Assertions.Any())
                {
                    _logger.LogWarning("Assertion batch has no assertions");
                    continue;
                }

                // Scene graph is updated per batch to group together updates
                try
                {
                    var update = await Process(assertionBatch);
                    await _renderWindow.SceneGraphUpdatesChannel.Writer.WriteAsync(update, _cancellationTokenManager.CancellationToken);
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
        public async Task<ISceneGraphUpdate> Process(AssertionBatch assertionBatch)
        {
            if (assertionBatch.Assertions.Count() == 1
                && assertionBatch.Assertions.Single() is CameraOrientation cameraOrientation)
            {
                return new ViewMatrixUpdate(GetViewMatrix(cameraOrientation));
            }
            else
            {
                SceneGraph sceneGraph = new SceneGraph();

                foreach (var assertion in assertionBatch.Assertions)
                {
                    if (assertion is Core.Assertions.ClearColor clearColor)
                    {
                        sceneGraph.RgbClearColor = new (clearColor.Red, clearColor.Green, clearColor.Blue, 1.0f);
                    }
                    else if (assertion is Core.Assertions.Vertex assertionVertex)
                    {
                        if (!sceneGraph.Vertices.TryGetValue(assertionVertex.VertexId, out Core.Scene.Vertex vertex))
                        {
                            vertex = new Core.Scene.Vertex(sceneGraph, assertionVertex.VertexId);
                            sceneGraph.Vertices[assertionVertex.VertexId] = vertex;
                        }
                        if (assertionVertex.Coordinates != default)
                        {
                            vertex.Coordinates = assertionVertex.Coordinates;
                        }
                    }
                    else if (assertion is Core.Assertions.Triangle triangle)
                    {
                        sceneGraph.Triangles[triangle.TriangleId] = new Core.Scene.Triangle(sceneGraph, triangle.TriangleId) { VertexIds = triangle.VertexIds };
                    }
                    else if (assertion is Core.Assertions.GrabScreenshot)
                    {
                        // Nothing to do here, screenshots passed out to event processor via render window event channel
                    }
                    else if (assertion is Core.Assertions.Texture texture)
                    {
                        // Pre-load texture to avoid doing this in the function setting OpenGL state
                        var loadedImage = await _imageLoader.LoadImage(new Uri(texture.Uri));
                        sceneGraph.Textures[texture.TextureId] = new Core.Scene.Texture(sceneGraph, texture.TextureId, loadedImage.BufferRgba,
                            loadedImage.Width, loadedImage.Height);
                    }
                    else if (assertion is Core.Assertions.TexCoords texCoords)
                    {
                        sceneGraph.SurfaceVertexTexCoords[texCoords.ObjectIdentifier] = texCoords;
                    }
                    else if (assertion is Core.Assertions.SurfaceColor surfaceColor)
                    {
                        sceneGraph.SurfaceVertexColors[surfaceColor.ObjectIdentifier] = surfaceColor.vertexColor;
                    }
                    else
                    {
                        throw new ArgumentException($"Unknown assertion type: {assertion.GetType()}");
                    }
                }

                return new ContentUpdate(sceneGraph);
            }
        }

        public Matrix4 GetViewMatrix(CameraOrientation cameraOrientation)
        {
            Vector3 eyePosition = new Vector3(cameraOrientation.CameraLocation.X, cameraOrientation.CameraLocation.Y, cameraOrientation.CameraLocation.Z);
            Vector3 eyeVector = new Vector3(cameraOrientation.CameraDirection.dX, cameraOrientation.CameraDirection.dY, cameraOrientation.CameraDirection.dZ);
            Vector3 upVector = new Vector3(cameraOrientation.UpDirection.dX, cameraOrientation.UpDirection.dY, cameraOrientation.UpDirection.dZ);
            var target = eyePosition + eyeVector;
            return Matrix4.LookAt(eyePosition, target, upVector);
        }
    }
}
