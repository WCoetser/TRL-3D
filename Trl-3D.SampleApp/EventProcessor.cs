using Microsoft.Extensions.Logging;

using System;
using System.IO;
using System.Threading.Tasks;
using System.Diagnostics;

using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Image = SixLabors.ImageSharp.Image;

using OpenTK.Windowing.GraphicsLibraryFramework;

using Trl_3D.Core.Abstractions;
using Trl_3D.Core.Events;
using Trl_3D.Core.Assertions;
using OpenTK.Mathematics;

namespace Trl_3D.SampleApp
{
    public class EventProcessor : IEventProcessor
    {
        private readonly IRenderWindow _renderWindow;
        private readonly ILogger<EventProcessor> _logger;
        private readonly ICancellationTokenManager _cancellationTokenManager;
        private readonly IAssertionProcessor _scene;

        private CameraOrientation _currentCameraOrientation;

        bool screenshotEnqueued = false;

        public EventProcessor(IRenderWindow renderWindow, 
                            ILogger<EventProcessor> logger, 
                            ICancellationTokenManager cancellationTokenManager, 
                            IAssertionProcessor scene)
        {
            _renderWindow = renderWindow;
            _logger = logger;
            _cancellationTokenManager = cancellationTokenManager;
            _scene = scene;
            _currentCameraOrientation = CameraOrientation.Default;
            _logger.LogInformation("EventProcessor created");
        }
        
        public async Task StartEventProcessor()
        {
            _logger.LogInformation("EventProcessor started");
            await foreach (var currenEvent in _renderWindow.EventChannel.Reader.ReadAllAsync(_cancellationTokenManager.CancellationToken)
                .WithCancellation(_cancellationTokenManager.CancellationToken))
            {
                try
                {
                    if (currenEvent is ScreenCaptureEvent captureEvent)
                    {
                        ProcessCapture(captureEvent.RgbBuffer, captureEvent.Width, captureEvent.Height);
                    }
                    else if (currenEvent is UserInputStateEvent userInputEvent)
                    {
                        await ProcessUserEvent(userInputEvent);
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

        private async Task ProcessUserEvent(UserInputStateEvent userInputEvent)
        {
            // Escape = quit
            if (userInputEvent.KeyboardState.WasKeyDown(Keys.Escape))
            {
                _renderWindow.Close();
            }

            // Take screenshot
            if (userInputEvent.KeyboardState.WasKeyDown(Keys.S))
            {
                if (!screenshotEnqueued)
                {
                    await _scene.AssertionUpdatesChannel.Writer.WriteAsync(new AssertionBatch
                    {
                        Assertions = new IAssertion[]
                        {
                            new GrabScreenshot()
                        }
                    });
                    screenshotEnqueued = true;
                }
            }

            // Move left & right
            bool hasLeft = userInputEvent.KeyboardState.IsKeyDown(Keys.A);
            bool hasRight = userInputEvent.KeyboardState.IsKeyDown(Keys.D);
            if (hasLeft || hasRight)
            {
                var dX = (hasLeft, hasRight) switch {
                    (true, false) => -1.0f,
                    (false, true) => 1.0f,
                    _ => 0.0f
                };
                dX *= (float)userInputEvent.TimeSinceLastEventSeconds;
                Vector3 moveVec = new (-1,0,0); // we are looking in the negative z direction, therefore "right" is -1
                moveVec *= dX;
                Vector3 newLocation = _currentCameraOrientation.CameraLocation.ToOpenTkVec3() + moveVec;

                _currentCameraOrientation = _currentCameraOrientation with
                {
                    CameraLocation = new (newLocation.X, newLocation.Y, newLocation.Z)
                };

                await _scene.AssertionUpdatesChannel.Writer.WriteAsync(new AssertionBatch
                {
                    Assertions = new IAssertion[]
                    {
                        _currentCameraOrientation
                    }
                }); 
            }
        }

        private void ProcessCapture(byte[] bufferRgb, int width, int height)
        {
            var filename = $"capture.png";

            var fileInfo = new FileInfo(filename);
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }

            var stopwatch = new Stopwatch();
            stopwatch.Start();

            using (var image = Image.LoadPixelData<Rgb24>(bufferRgb, width, height))
            {
                image.Mutate(x => x.RotateFlip(RotateMode.None, FlipMode.Vertical));
                image.SaveAsPng(fileInfo.FullName);
            }

            stopwatch.Stop();

            _logger.LogInformation($"Captured to {fileInfo.FullName} in {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}
