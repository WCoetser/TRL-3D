using Microsoft.Extensions.Logging;

using System;
using System.Linq;
using System.Threading.Channels;
using System.Threading.Tasks;

using Trl_3D.Core.Abstractions;

namespace Trl_3D.Core.Scene
{
    public class Scene : IScene 
    { 
        private readonly ILogger<Scene> _logger;
        private readonly IRenderWindow _renderWindow;
        private readonly ICancellationTokenManager _cancellationTokenManager;
        private readonly AssertionProcessor _assertionProcessor;

        public Channel<AssertionBatch> AssertionUpdatesChannel { get; private set; }

        public Scene(ILogger<Scene> logger,
                     IRenderWindow renderWindow,
                     ICancellationTokenManager cancellationTokenManager,
                     AssertionProcessor assertionProcessor)
        {
            _logger = logger;
            _renderWindow = renderWindow;
            _cancellationTokenManager = cancellationTokenManager;
            _assertionProcessor = assertionProcessor;

            AssertionUpdatesChannel = Channel.CreateUnbounded<AssertionBatch>();

            _logger.LogInformation("Scene created.");
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
                    var update = await _assertionProcessor.Process(assertionBatch);
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
    }
}
