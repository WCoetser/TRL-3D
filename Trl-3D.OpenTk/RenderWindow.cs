﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTK.Graphics.ES30;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System;
using System.Collections.Generic;
using Trl_3D.Core.Abstractions;

namespace Trl_3D.OpenTk
{
    public class RenderWindow : GameWindow, IRenderWindow
    {
        private ILogger _logger;
        private ISceneLoader _loader;
        private OpenGLSceneProcessor _openGLSceneProcessor;

        public RenderWindow(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            Load += MainWindowLoad;
            Resize += MainWindowResize;
            RenderFrame += MainWindowRenderFrame;
            UpdateFrame += MainWindowUpdateFrame;
        }

        private void MainWindowUpdateFrame(FrameEventArgs obj)
        {           
            if (KeyboardState.IsKeyDown(Keys.Escape))
            {
                Close();
            }
        }

        private void MainWindowRenderFrame(FrameEventArgs e)
        {
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
            _logger.LogInformation($"Open GL version: {GL.GetString(StringName.Version)}");
            IEnumerable<IAssertion> assertions = _loader.LoadInitialScene();
            _openGLSceneProcessor.SetState(assertions);

            Closed += MainWindowClosed;

            _logger.LogInformation("RenderWindow Load complete");
        }

        public void Initialize(IServiceProvider serviceProvider)
        {
            _logger = serviceProvider.GetRequiredService<ILogger<RenderWindow>>();
            _loader = serviceProvider.GetRequiredService<ISceneLoader>();
            _openGLSceneProcessor = new OpenGLSceneProcessor(serviceProvider);
        }
    }
}
