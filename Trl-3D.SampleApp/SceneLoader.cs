using Microsoft.Extensions.Logging;

using Trl_3D.Core.Assertions;
using Trl_3D.Core.Abstractions;

using System.Threading.Tasks;
//using Trl_3D.Core.Scene;

namespace Trl_3D.SampleApp
{
    public class SceneLoader : IAssertionLoader
    {
        private readonly ILogger<SceneLoader> _logger;
        private readonly IScene _scene;
        private readonly ICancellationTokenManager _cancellationTokenManager;

        public SceneLoader(ILogger<SceneLoader> logger, IScene scene, ICancellationTokenManager cancellationTokenManager)
        {
            _logger = logger;
            _scene = scene;
            _cancellationTokenManager = cancellationTokenManager;
            _logger.LogInformation("SceneLoader created");
        }

        public async Task StartAssertionProducer()
        {
            var batch1 = new AssertionBatch
            {
                Assertions = new IAssertion[]
                {
                    // will always execute at the start of the render loop
                    new ClearColor(0.1f, 0.1f, 0.2f),
                    // will always execute at the end of the render loop
                    new GrabScreenshot()
                }
            };
            await _scene.AssertionUpdatesChannel.Writer.WriteAsync(batch1, _cancellationTokenManager.CancellationToken);
                       
            var batch2 = new AssertionBatch
            {
                // TODO: Use IDs to cater for external integration, display indices
                Assertions = new IAssertion[]
                {
                    // Top
                    new Vertex(0, new Coordinate3d(-0.25f, 0.0f, 0.0f)), new Vertex(0, new ColorRgba(1.0f, 0.0f, 0.0f, 1.0f)),
                    new Vertex(1, new Coordinate3d(0.25f, 0.0f, 0.0f)),  new Vertex(1, new ColorRgba(1.0f, 0.0f, 0.0f, 1.0f)),
                    new Vertex(2, new Coordinate3d(0.0f,  0.25f, 0.0f)), new Vertex(2, new ColorRgba(1.0f, 0.0f, 0.0f, 1.0f)),
                    new Triangle(3, (0, 1, 2)),
            
                    // Bottom right
                    new Vertex(4, new Coordinate3d(0.5f, -0.25f, 0.0f)), new Vertex(4, new ColorRgba(0.0f, 1.0f, 0.0f, 1.0f)),
                    new Vertex(5, new Coordinate3d(0.0f, -0.25f, 0.0f)), new Vertex(5, new ColorRgba(0.0f, 1.0f, 0.0f, 1.0f)),
                    new Triangle(6, (4, 5, 1)), // note - vertices re-used

                    // Bottom left
                    new Vertex(7, new Coordinate3d(-0.5f, -0.25f, 0.0f)), new Vertex(7, new ColorRgba(0.0f, 0.0f, 1.0f, 1.0f)),
                    new Triangle(8, (5, 7, 0)), // note - vertices re-used
                }
            };
            await _scene.AssertionUpdatesChannel.Writer.WriteAsync(batch2, _cancellationTokenManager.CancellationToken);

            _scene.AssertionUpdatesChannel.Writer.Complete();

            _logger.LogInformation("Scene assertion producer stopped.");
        }        
    }
}
