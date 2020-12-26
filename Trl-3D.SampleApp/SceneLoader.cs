using Microsoft.Extensions.Logging;

using Trl_3D.Core.Assertions;
using Trl_3D.Core.Abstractions;

using System.Threading.Tasks;

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
            var batch = new AssertionBatch
            {
                Assertions = new IAssertion[]
                {
                    new ClearColor(0.1f, 0.1f, 0.2f),
                    new RenderTestTriagle(),
                    new GrabScreenshot()
                }
            };

            await _scene.AssertionUpdatesChannel.Writer.WriteAsync(batch, _cancellationTokenManager.CancellationToken);
            
            // TODO: Remove this when implementing differential rendering
            _scene.AssertionUpdatesChannel.Writer.Complete();

            _logger.LogInformation("Scene assertion producer stopped.");
        }        
    }
}
