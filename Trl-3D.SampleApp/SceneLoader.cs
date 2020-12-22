using Microsoft.Extensions.Logging;

using Trl_3D.Core.Assertions;
using Trl_3D.Core.Abstractions;

using System.Threading.Tasks;
using System.Threading;

namespace Trl_3D.SampleApp
{
    public class SceneLoader : IAssertionLoader
    {
        private readonly ILogger<SceneLoader> _logger;
        private readonly IScene _scene;

        public SceneLoader(ILogger<SceneLoader> logger, IScene scene)
        {
            _logger = logger;
            _scene = scene;
            _logger.LogInformation("SceneLoader created");
        }

        public async Task StartAssertionProducer(CancellationToken cancellationToken)
        {
            await _scene.AssertionUpdatesChannel.Writer.WriteAsync(new ClearColor(0.1f, 0.1f, 0.2f), cancellationToken);
            await _scene.AssertionUpdatesChannel.Writer.WriteAsync(new RenderTestTriagle(), cancellationToken);
            await _scene.AssertionUpdatesChannel.Writer.WriteAsync(new GrabScreenshot(), cancellationToken);

            // TODO: Remove this when implementing differential rendering
            _scene.AssertionUpdatesChannel.Writer.Complete();

            _logger.LogInformation("Scene assertion producer stopped.");
        }        
    }
}
