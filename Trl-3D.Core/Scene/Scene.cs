using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Trl_3D.Core.Abstractions;

namespace Trl_3D.Core.Scene
{
    public class Scene : IScene 
    { 
        private readonly ILogger<Scene> _logger;
        private readonly IRenderWindow _renderWindow;
        private readonly AssertionProcessor _assertionProcessor;

        public Channel<IAssertion> AssertionUpdatesChannel { get; private set; }

        public Scene(ILogger<Scene> logger,
                     IRenderWindow renderWindow)
        {
            _logger = logger;
            _renderWindow = renderWindow;
            _assertionProcessor = new AssertionProcessor();

            AssertionUpdatesChannel = Channel.CreateUnbounded<IAssertion>();

            _logger.LogInformation("Scene created.");
        }

        public async Task StartAssertionConsumer(CancellationToken cancellationToken)
        {
            // TODO: Add differential rendering
            SceneGraph sceneGraph = new SceneGraph();

            await foreach (var assertion in AssertionUpdatesChannel.Reader.ReadAllAsync(cancellationToken))
            {
                _logger.LogInformation($"Received {assertion.GetType().Name}");
                _assertionProcessor.Process(assertion, sceneGraph);
            }

            // TODO: Move updates to await foreach loop
            await _renderWindow.SceneGraphUpdatesChannel.Writer.WriteAsync(sceneGraph, cancellationToken);

            _logger.LogInformation("Scene assertion consumer stopped.");
        }
    }
}
