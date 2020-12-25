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
using OpenTK.Windowing.GraphicsLibraryFramework;

namespace Trl_3D.SampleApp
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IRenderWindow _renderWindow;
        private readonly ILogger<EventProcessor> _logger;
        private readonly ICancellationTokenManager _cancellationTokenManager;

        public EventProcessor(IRenderWindow renderWindow, ILogger<EventProcessor> logger, ICancellationTokenManager cancellationTokenManager)
        {
            _renderWindow = renderWindow;
            _logger = logger;
            _cancellationTokenManager = cancellationTokenManager;
            _logger.LogInformation("EventProcessor created");
        }

        public async Task StartEventProcessor()
        {
            _logger.LogInformation("EventProcessor started");
            await foreach (var currenEvent in _renderWindow.EventChannel.Reader.ReadAllAsync(_cancellationTokenManager.CancellationToken))
            {
                try
                {
                    if (currenEvent is ScreenCaptureEvent captureEvent)
                    {
                        ProcessCapture(captureEvent.RgbBuffer, captureEvent.Width, captureEvent.Height);
                    }
                    else if (currenEvent is UserInputStateEvent userInputEvent)
                    {
                        ProcessUserEvent(userInputEvent);
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

        private void ProcessUserEvent(UserInputStateEvent userInputEvent)
        {
            if (userInputEvent.KeyboardState.WasKeyDown(Keys.Escape))
            {
                _renderWindow.Close();
            }
        }

        private void ProcessCapture(byte[] buffer, int width, int height)
        {
            var filename = $"capture.png";

            var fileInfo = new FileInfo(filename);
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }

            using var image = SixLabors.ImageSharp.Image.LoadPixelData<Rgb24>(buffer, width, height);

            image.Mutate(x => x.RotateFlip(RotateMode.None, FlipMode.Vertical));
            image.SaveAsPng(fileInfo.FullName);

            _logger.LogInformation($"Captured to {fileInfo.FullName}");
        }
    }
}
