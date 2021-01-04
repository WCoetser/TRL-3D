using Microsoft.Extensions.Logging;

using Trl_3D.Core.Assertions;
using Trl_3D.Core.Abstractions;

using System.Threading.Tasks;
using System;
using System.IO;

namespace Trl_3D.SampleApp
{
    public class SceneLoader : IAssertionLoader
    {
        private readonly ILogger<SceneLoader> _logger;
        private readonly IAssertionProcessor _scene;
        private readonly ICancellationTokenManager _cancellationTokenManager;

        public SceneLoader(ILogger<SceneLoader> logger, 
                            IAssertionProcessor scene, 
                            ICancellationTokenManager cancellationTokenManager)
        {
            _logger = logger;
            _scene = scene;
            _cancellationTokenManager = cancellationTokenManager;
            _logger.LogInformation("SceneLoader created");
        }

        public async Task StartAssertionProducer()
        {
            var currentLocation = new FileInfo(typeof(SceneLoader).Assembly.Location);
            string image = $"{Uri.UriSchemeFile}://{currentLocation.Directory}/snail.jpg";
            string image2 = $"{Uri.UriSchemeFile}://{currentLocation.Directory}/snail_2.jpg";

            var batch1 = new AssertionBatch
            {
                Assertions = new IAssertion[]
                {
                    // Initial setup
                    new ClearColor(0.1f, 0.1f, 0.2f),
                    Constants.DefaultCameraOrientation,
                    new CameraProjectionPerspective(90, 0.3f, 3),

                    // Images
                    new Texture(100, image),
                    new Texture(200, image2),

                    // Define vertices
                    new Vertex(0, new Coordinate3d(-0.33f, 0.0f, 0.0f)),
                    new Vertex(1, new Coordinate3d(0.33f, 0.0f, 0.0f)),
                    new Vertex(2, new Coordinate3d(0.0f,  0.33f, 0.0f)),
                    new Vertex(4, new Coordinate3d(0.66f, -0.33f, 0.0f)),
                    new Vertex(5, new Coordinate3d(0.0f, -0.33f, 0.0f)),
                    new Vertex(7, new Coordinate3d(-0.66f, -0.33f, 0.0f)),

                    // Top triangle
                    new Triangle(3, (0, 1, 2)),
                    new TexCoords((3, 0), 100, 0f, 0.2f),
                    new TexCoords((3, 1), 100, 1f, 0.2f),
                    new TexCoords((3, 2), 100, 0.5f,  0.7f),
            
                    // Bottom right triangle
                    new Triangle(6, (4, 5, 1)),
                    new TexCoords((6, 4), 200, 0.8f, 0.5f),
                    new TexCoords((6, 5), 200, -0.2f, 0.5f),
                    new TexCoords((6, 1), 200, 0.3f, 1f),

                    // Bottom left triangle
                    new Triangle(8, (5, 7, 0)),
                    new SurfaceColor((8, 7), new ( 1.0f, 0.0f, 0.0f, 1.0f)),
                    new SurfaceColor((8, 5), new ( 0.0f, 1.0f, 0.0f, 1.0f)),
                    new SurfaceColor((8, 0), new ( 0.0f, 0.0f, 1.0f, 1.0f))
                }
            };

            // Send the assertions to the rendering system
            await _scene.AssertionUpdatesChannel.Writer.WriteAsync(batch1, _cancellationTokenManager.CancellationToken);

            // Render second test batch
            var batch2 = new AssertionBatch
            {
                Assertions = new IAssertion[]
                {
                    // Define vertices
                    new Vertex(9, new Coordinate3d(-0.33f, 0.0f, -1.0f)),
                    new Vertex(10, new Coordinate3d(0.33f, 0.0f, -1.0f)),
                    new Vertex(11, new Coordinate3d(0.0f,  -0.33f, -1.0f)),

                    // Bottom left triangle
                    new Triangle(12, (9, 10, 11)),
                    new SurfaceColor((12, 9), new ( 1.0f, 1.0f, 0.0f, 1.0f)),
                    new SurfaceColor((12, 10), new ( 0.0f, 1.0f, 1.0f, 1.0f)),
                    new SurfaceColor((12, 11), new ( 1.0f, 0.0f, 1.0f, 1.0f))
                }
            };

            // Send the assertions to the rendering system
            await _scene.AssertionUpdatesChannel.Writer.WriteAsync(batch2, _cancellationTokenManager.CancellationToken);
        }
    }
}
