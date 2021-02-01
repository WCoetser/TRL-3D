using Microsoft.Extensions.Logging;
using OpenTK.Mathematics;
using System;
using System.Threading.Tasks;
using Trl_3D.Core.Abstractions;
using Trl_3D.Core.Assertions;

namespace Trl_3D.SampleApp
{
    /// <summary>
    /// Test class using the rest of the dependencies to do animations
    /// on a seperate thread, updating vertex postions.
    /// </summary>
    public class Animation
    {
        private readonly ILogger<Animation> _logger;
        private readonly IAssertionProcessor _scene;
        private readonly ICancellationTokenManager _cancellationTokenManager;

        public Animation(ILogger<Animation> logger,
                            IAssertionProcessor scene,
                            ICancellationTokenManager cancellationTokenManager)
        {
            _logger = logger;
            _scene = scene;
            _cancellationTokenManager = cancellationTokenManager;
        }

        public async Task Start()
        {
            _logger.LogInformation("Animation thread started");

            // Hackish way to measure time ...
            float totalTime = 0;
            const float target_fps = 60;
            int threadDelay = (int)Math.Ceiling(1/target_fps);

            while (!_cancellationTokenManager.IsCancellationRequested)
            {
                // Update triangle vertex positions and send to update channel
                // so that their positions are updated and vertex buffers are refreshed

                var angleDegrees = totalTime / 2.0;

                var angle = MathHelper.DegreesToRadians(angleDegrees);
                var modelTransformMatrix = Matrix4.CreateRotationZ((float)angle);

                // Rotate the triangle by updating vertex positions, which will update the vertex buffers on
                // the renderer
                var assetionBatch = new AssertionBatch
                {
                    Assertions = new IAssertion[]
                    {                        
                        new Vertex(0, new Coordinate3d(-0.33f, 0.0f, 0.0f).Transform(modelTransformMatrix)),
                        new Vertex(1, new Coordinate3d(0.33f, 0.0f, 0.0f).Transform(modelTransformMatrix)),
                        new Vertex(2, new Coordinate3d(0.0f,  0.33f, 0.0f).Transform(modelTransformMatrix)),
                        new Vertex(4, new Coordinate3d(0.66f, -0.33f, 0.0f).Transform(modelTransformMatrix)),
                        new Vertex(5, new Coordinate3d(0.0f, -0.33f, 0.0f).Transform(modelTransformMatrix)),
                        new Vertex(7, new Coordinate3d(-0.66f, -0.33f, 0.0f).Transform(modelTransformMatrix))
                    }
                };
                await _scene.AssertionUpdatesChannel.Writer.WriteAsync(assetionBatch, _cancellationTokenManager.CancellationToken);
                await Task.Delay(threadDelay);

                totalTime += threadDelay;
            }
        }        
    }
}
