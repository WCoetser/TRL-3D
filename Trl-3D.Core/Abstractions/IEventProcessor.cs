namespace Trl_3D.Core.Abstractions
{
    public interface IEventProcessor
    {
        /// <summary>
        /// Receives the last thing rendered.
        /// <seealso cref="Assertions.GrabFrameBuffer"/> needs to be sent to the command queue for this event to be raised.
        /// </summary>
        public void ReceiveFrameBuffer(byte[] renderedImage);
    }
}
