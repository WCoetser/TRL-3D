using Trl_3D.Core.Abstractions;
using OpenTK.Graphics.OpenGL4;
using System;

namespace Trl_3D.OpenTk.Assertions
{
    public delegate void CaptureCallback(byte[] bufferOut, RenderInfo time);

    public class GrabScreenshot : IAssertion
    {
        public RenderProcessStep ProcessStep => RenderProcessStep.End;

        /// <summary>
        /// Called onnce a screenshot is captured.
        /// </summary>
        public CaptureCallback CaptureCallback { get; set; }

        public bool SelfDestruct => true;

        public void Dispose()
        {
            // nothing to do here
        }

        public void Render(RenderInfo renderInfo)
        {
            _ = CaptureCallback ?? throw new ArgumentNullException(nameof(CaptureCallback));

            byte[] backBufferDump = new byte[renderInfo.Width * renderInfo.Height * 3];
            GL.ReadBuffer(ReadBufferMode.Back);
            GL.ReadPixels(0, 0, renderInfo.Width, renderInfo.Height, PixelFormat.Rgb, PixelType.Byte, backBufferDump);

            CaptureCallback(backBufferDump, renderInfo.Clone());
        }

        public void SetState()
        {
            // nothing to do here
        }
    }
}
