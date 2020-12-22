using Microsoft.Extensions.Logging;

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Trl_3D.Core.Abstractions;

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Trl_3D.Core.Events;

namespace Trl_3D.SampleApp
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IRenderWindow _renderWindow;
        private readonly ILogger<EventProcessor> _logger;

        public EventProcessor(IRenderWindow renderWindow, ILogger<EventProcessor> logger)
        {
            _renderWindow = renderWindow;
            _logger = logger;
            _logger.LogInformation("EventProcessor created");
        }

        public async Task StartEventProcessor(CancellationToken cancelationToken)
        {
            _logger.LogInformation("EventProcessor started");
            await foreach (var evt in _renderWindow.EventChannel.Reader.ReadAllAsync(cancelationToken))
            {
                try
                {
                    // TODO: Remove
                    _logger.LogInformation($"Received {evt.GetType().FullName}");

                    if (evt is ScreenCaptureEvent sce)
                    {
                        ProcessCapture(sce.RgbBuffer, sce.Width, sce.Height);
                    }
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Event processor failed");
                }
            }
            _logger.LogInformation("EventProcessor stopped");
        }

        private void ProcessCapture(byte[] buffer, int width, int height)
        {
            var filename = $"capture.png";

            var fileInfo = new FileInfo(filename);
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }

            using var image = Image.LoadPixelData<Rgb24>(buffer, width, height);

            image.Mutate(x => x.RotateFlip(RotateMode.None, FlipMode.Vertical));
            image.SaveAsPng(fileInfo.FullName);

            _logger.LogInformation($"Captured to {fileInfo.FullName}");
        }
    }
}
