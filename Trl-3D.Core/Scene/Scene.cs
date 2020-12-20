using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

using Trl_3D.Core.Abstractions;

namespace Trl_3D.Core.Scene
{
    public class Scene : IScene 
    { 
        private readonly IAssertionLoader _assertionLoader;
        private readonly ILogger<Scene> _logger;
        private readonly IRenderWindow _renderWindow;
        private readonly AssertionProcessor _assertionProcessor;

        public Scene(IAssertionLoader assertionLoader,
                     ILogger<Scene> logger,
                     IRenderWindow renderWindow)
        {
            _assertionLoader = assertionLoader;
            _logger = logger;
            _renderWindow = renderWindow;
            _assertionProcessor = new AssertionProcessor();
        }

        public async Task StartAssertionConsumer(CancellationToken cancellationToken)
        {
            // TODO: Add differential rendering
            SceneGraph sceneGraph = new SceneGraph();

            await foreach (var assertion in _assertionLoader.AssertionUpdatesChannel.Reader.ReadAllAsync(cancellationToken))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
                _logger.LogInformation($"Received {assertion.GetType().Name}");
                _assertionProcessor.Process(assertion, sceneGraph);
            }

            await _renderWindow.SceneGraphUpdatesChannel.Writer.WriteAsync(sceneGraph, cancellationToken);

            _logger.LogInformation("Scene assertion consumer stopped.");
        }
    }
}
