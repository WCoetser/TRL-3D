using Trl_3D.Core.Abstractions;

namespace Trl_3D.SampleApp
{
    public class EventProcessor : IEventProcessor
    {
        public void ReceiveFrameBuffer(byte[] renderedImage)
        {
            System.Diagnostics.Debugger.Break();
        }
    }
}
