using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using System;
using System.Threading.Channels;
using Trl_3D.Core.Abstractions;
using OpenTK.Graphics.OpenGL4;
using Trl_3D.Core.Events;
using System.Threading.Tasks;
using Trl_3D.Core.Scene;
using System.Threading;

namespace Trl_3D.OpenTk
{
    public class RenderWindow : GameWindow, IRenderWindow
    {
        private ILogger _logger;
        private OpenGLSceneProcessor _openGLSceneProcessor;
        private CancellationTokenSource _cancellationTokenManager;

        public Channel<IRenderCommand> RenderCommandUpdatesChannel { get; private set; }

        public Channel<IEvent> EventChannel { get; private set; }

        // Release resources without overlapping with rendering operations
        private object _updateFrameLock = new object();
        private object _renderFrameLock = new object();
        private object _resizeWindowLock = new object();
        private bool _shutdownInProgress = false;

        public RenderWindow(GameWindowSettings gameWindowSettings, 
                            NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            Load += MainWindowLoad;
            Resize += MainWindowResize;
            RenderFrame += MainWindowRenderFrame;
            UpdateFrame += MainWindowUpdateFrame;

            RenderCommandUpdatesChannel = Channel.CreateUnbounded<IRenderCommand>();
            EventChannel = Channel.CreateUnbounded<IEvent>();
        }

        private void MainWindowUpdateFrame(FrameEventArgs obj)
        {
            // Use seperate locks for update and render to avoid stuttering
            lock (_updateFrameLock)
            {
                if (_shutdownInProgress)
                {
                    return;
                }

                var userEvent = new UserInputStateEvent(KeyboardState.GetSnapshot(), MouseState.GetSnapshot(), obj.Time);
                var task = Task.Run(async () =>
                {
                    await EventChannel.Writer.WriteAsync(userEvent, _cancellationTokenManager.Token).AsTask();
                });
                task.Wait(_cancellationTokenManager.Token);
            }
        }

        private void MainWindowRenderFrame(FrameEventArgs e)
        {
            // Use seperate locks for update and render to avoid stuttering
            lock (_renderFrameLock)
            {
                if (_shutdownInProgress)
                {
                    return;
                }

                // This should only update the OpenGL state when there are inputs in the scene graph update channel
                while (RenderCommandUpdatesChannel.Reader.TryRead(out IRenderCommand renderCommand))
                {
                    _openGLSceneProcessor.UpdateState(renderCommand);
                }

                _openGLSceneProcessor.Render(e.Time);
                SwapBuffers();
            }
        }

        private void MainWindowResize(ResizeEventArgs obj)
        {
            lock (_resizeWindowLock)
            {
                _openGLSceneProcessor.ResizeRenderWindow(obj.Width, obj.Height);
            }
        }

        private void MainWindowLoad()
        {
            Closing += MainWindowClosing;

            _logger.LogInformation($"Open GL version: {GL.GetString(StringName.Version)}");
            _logger.LogInformation("RenderWindow Load complete");
        }

        private void MainWindowClosing(System.ComponentModel.CancelEventArgs obj)
        {
            ReleaseResources();
        }

        public void Initialize(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<RenderWindow>>();
            var sceneGraph = serviceProvider.GetRequiredService<SceneGraph>();
            _openGLSceneProcessor = new OpenGLSceneProcessor(serviceProvider, this, sceneGraph);
            _cancellationTokenManager = serviceProvider.GetRequiredService<CancellationTokenSource>();
        }

        protected void ReleaseResources()
        {
            lock (_resizeWindowLock)
            {
                lock (_updateFrameLock)
                {
                    lock (_renderFrameLock)             
                    {
                        if (_shutdownInProgress)
                        {
                            return;
                        }

                        _shutdownInProgress = true;
                        _logger.LogInformation("RenderWindow release resources started");
                        _openGLSceneProcessor.ReleaseResources();
                        _logger.LogInformation("RenderWindow release resources ended");

                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            // This should not be needed if window is closed normally
            ReleaseResources();

            base.Dispose(disposing);
        }
    }
}
