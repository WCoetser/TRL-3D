﻿using Trl_3D.Core.Abstractions;

using OpenTK.Graphics.OpenGL4;
using Trl_3D.Core.Events;
using System.Threading;

namespace Trl_3D.OpenTk.RenderCommands
{
    public class GrabScreenshot : IRenderCommand
    {
        private readonly IRenderWindow _renderWindow;
        private readonly CancellationTokenSource _cancellationTokenManager;

        public RenderProcessPosition ProcessStep => RenderProcessPosition.AfterContent;

        public GrabScreenshot(IRenderWindow renderWindow, CancellationTokenSource cancellationTokenManager)
        {
            _renderWindow = renderWindow;
            _cancellationTokenManager = cancellationTokenManager;
        }

        public bool SelfDestruct => true;

        public void Dispose()
        {
            // nothing to do here
        }

        public void Render(RenderInfo renderInfo)
        {
            byte[] backBufferDump = new byte[renderInfo.Width * renderInfo.Height * 3];
            GL.ReadBuffer(ReadBufferMode.ColorAttachment0);
            GL.ReadPixels(0, 0, renderInfo.Width, renderInfo.Height, PixelFormat.Rgb, PixelType.UnsignedByte, backBufferDump);

            var screenCaptureEvent = new ScreenCaptureEvent(backBufferDump, renderInfo.Width, renderInfo.Height);
            var t = _renderWindow.EventChannel.Writer.WriteAsync(screenCaptureEvent, _cancellationTokenManager.Token).AsTask();
            t.Wait(_cancellationTokenManager.Token);
        }

        public void SetState(RenderInfo renderInfo)
        {
            // nothing to do here
        }
    }
}
