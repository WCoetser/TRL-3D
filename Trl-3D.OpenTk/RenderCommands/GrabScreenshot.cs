using System;
using Trl_3D.Core.Abstractions;
using Trl_3D.Core.Assertions;

using OpenTK.Graphics.OpenGL4;

namespace Trl_3D.OpenTk.RenderCommands
{
    public class GrabScreenshot : IRenderCommand
    {
        public RenderProcessPosition ProcessStep => RenderProcessPosition.AfterContent;

        private Action<byte[], int, int> _captureCallback;

        public GrabScreenshot(Action<byte[], int, int> captureCallback)
        {
            _captureCallback = captureCallback;
        }

        public bool SelfDestruct => true;

        public void Dispose()
        {
            // nothing to do here
        }

        public void Render(RenderInfo renderInfo)
        {
            _ = _captureCallback ?? throw new ArgumentNullException();

            byte[] backBufferDump = new byte[renderInfo.Width * renderInfo.Height * 3];
            GL.ReadBuffer(ReadBufferMode.Back);
            GL.ReadPixels(0, 0, renderInfo.Width, renderInfo.Height, PixelFormat.Rgb, PixelType.UnsignedByte, backBufferDump);

            _captureCallback(backBufferDump, renderInfo.Width, renderInfo.Height);
        }

        public void SetState()
        {
            // nothiong to do here
        }
    }
}
