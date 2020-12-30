using Microsoft.Extensions.Logging;

using System;
using System.IO;
using System.Threading.Tasks;

using Trl_3D.Core.Abstractions;

using Trl_3D.Core.Events;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Drawing;
using System.Drawing.Imaging;

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
                    else
                    {
                        _logger.LogWarning($"Unknown event type {currenEvent.GetType().FullName}");
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

            using (Bitmap bmp = new Bitmap(width, height))
            {
                for (int x = 0; x < width; x++)
                {
                    for (int y = 0; y < height; y++)
                    {
                        var bufferAddress = (y * width + x) * 3;
                        byte red = buffer[bufferAddress];
                        byte green = buffer[bufferAddress + 1];
                        byte blue = buffer[bufferAddress + 2];

                        var y_inverted = (height - 1) - y;

                        bmp.SetPixel(x, y_inverted, Color.FromArgb(red, green, blue));
                    }
                }

                bmp.Save(fileInfo.FullName, ImageFormat.Png);
            }

            _logger.LogInformation($"Captured to {fileInfo.FullName}");
        }
    }
}
