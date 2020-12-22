using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTK.Graphics.ES30;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Threading.Channels;
using Trl_3D.Core.Abstractions;
using Trl_3D.Core.Scene;

namespace Trl_3D.OpenTk
{
    public class RenderWindow : GameWindow, IRenderWindow
    {
        private ILogger _logger;
        private OpenGLSceneProcessor _openGLSceneProcessor;

        public Channel<SceneGraph> SceneGraphUpdatesChannel { get; private set; }

        public Channel<IEvent> EventChannel { get; private set; }

        public RenderWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            Load += MainWindowLoad;
            Resize += MainWindowResize;
            RenderFrame += MainWindowRenderFrame;
            UpdateFrame += MainWindowUpdateFrame;

            SceneGraphUpdatesChannel = Channel.CreateUnbounded<SceneGraph>();
            EventChannel = Channel.CreateUnbounded<IEvent>();
        }

        private void MainWindowUpdateFrame(FrameEventArgs obj)
        {
            // NB: OpenGL state cannot be directly updated from this method
            
            // TODO: Pass updates to ISceneLoader.AssertionUpdatesChannel?

            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Close();
            }
        }

        private void MainWindowRenderFrame(FrameEventArgs e)
        {
            // TODO: Add differential rendering
            while (SceneGraphUpdatesChannel.Reader.TryRead(out SceneGraph sceneGraph))
            {
                _openGLSceneProcessor.UpdateState(sceneGraph);
            }

            _openGLSceneProcessor.Render(e.Time);
            SwapBuffers();
        }

        private void MainWindowResize(ResizeEventArgs obj)
        {
            _openGLSceneProcessor.ResizeRenderWindow(obj.Width, obj.Height);
        }

        private void MainWindowClosed()
        {
            _openGLSceneProcessor.ReleaseResources();
            _logger.LogInformation("RenderWindow Closed complete");
        }

        private void MainWindowLoad()
        {
            Closed += MainWindowClosed;

            _logger.LogInformation($"Open GL version: {GL.GetString(StringName.Version)}");
            _logger.LogInformation("RenderWindow Load complete");
        }

        public void Initialize(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<RenderWindow>>();
            _openGLSceneProcessor = new OpenGLSceneProcessor(serviceProvider, this);
        }
    }
}
