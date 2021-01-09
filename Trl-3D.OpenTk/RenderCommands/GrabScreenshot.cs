using Trl_3D.Core.Abstractions;

using OpenTK.Graphics.OpenGL4;
using Trl_3D.Core.Events;

namespace Trl_3D.OpenTk.RenderCommands
{
    public class GrabScreenshot : IRenderCommand
    {
        private readonly IRenderWindow _renderWindow;
        private readonly ICancellationTokenManager _cancellationTokenManager;

        public RenderProcessPosition ProcessStep => RenderProcessPosition.AfterContent;

        public GrabScreenshot(IRenderWindow renderWindow, ICancellationTokenManager cancellationTokenManager)
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
            GL.ReadBuffer(ReadBufferMode.Back);
            GL.ReadPixels(0, 0, renderInfo.Width, renderInfo.Height, PixelFormat.Rgb, PixelType.UnsignedByte, backBufferDump);

            var screenCaptureEvent = new ScreenCaptureEvent(backBufferDump, renderInfo.Width, renderInfo.Height);
            var t = _renderWindow.EventChannel.Writer.WriteAsync(screenCaptureEvent, _cancellationTokenManager.CancellationToken).AsTask();
            t.Wait(_cancellationTokenManager.CancellationToken);
        }

        public void SetState(RenderInfo renderInfo)
        {
            // nothiong to do here
        }

        public PickingInfo RenderForPicking(RenderInfo renderInfo, int screenX, int screenY)
        {
            return null;
        }
    }
}
