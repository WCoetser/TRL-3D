using System;
using Trl_3D.Core.Abstractions;
using Trl_3D.Core.Assertions;

using OpenTK.Graphics.OpenGL4;

namespace Trl_3D.OpenTk.RenderCommands
{
    public class GrabScreenshotCommand : IRenderCommand
    {
        public RenderProcessPosition ProcessStep => RenderProcessPosition.AfterContent;

        private CaptureCallback _captureCallback;

        public bool SelfDestruct => true;

        public Type AssociatedAssertionType => typeof(GrabScreenshot);

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

            _captureCallback(backBufferDump, renderInfo.Clone());
        }

        public void SetState(GrabScreenshot assertion)
        {
            _captureCallback = assertion.CaptureCallback;
        }

        public void SetState(IAssertion assertion)
        {
            var a = (GrabScreenshot)assertion;
            _captureCallback = a.CaptureCallback;
        }
    }
}
