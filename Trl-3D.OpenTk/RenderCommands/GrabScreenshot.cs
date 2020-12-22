using Trl_3D.Core.Abstractions;

using OpenTK.Graphics.OpenGL4;

using System.Threading.Tasks;
using Trl_3D.Core.Events;

namespace Trl_3D.OpenTk.RenderCommands
{
    public class GrabScreenshot : IRenderCommand
    {
        private readonly IRenderWindow _renderWindow;

        public RenderProcessPosition ProcessStep => RenderProcessPosition.AfterContent;

        public GrabScreenshot(IRenderWindow renderWindow)
        {
            _renderWindow = renderWindow;
        }

        public bool SelfDestruct => true;

        public void Dispose()
        {
            // nothing to do here
        }

        public void Render(RenderInfo renderInfo)
        {
            byte[] backBufferDump = new byte[renderInfo.Width * renderInfo.Height * 3];
            GL.ReadBuffer(ReadBufferMode.Back);
            GL.ReadPixels(0, 0, renderInfo.Width, renderInfo.Height, PixelFormat.Rgb, PixelType.UnsignedByte, backBufferDump);

            var screenCaptureEvent = new ScreenCaptureEvent
            {
                RgbBuffer = backBufferDump,
                Width = renderInfo.Width,
                Height = renderInfo.Height
            };
            var t = _renderWindow.EventChannel.Writer.WriteAsync(screenCaptureEvent).AsTask();
            t.Wait();
        }

        public void SetState()
        {
            // nothiong to do here
        }
    }
}
