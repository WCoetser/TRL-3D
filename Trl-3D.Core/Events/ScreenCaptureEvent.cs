using Trl_3D.Core.Abstractions;

namespace Trl_3D.Core.Events
{
    public class ScreenCaptureEvent: IEvent
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public byte[] RgbBuffer { get; set; }
    }
}
