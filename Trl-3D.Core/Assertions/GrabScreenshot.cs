using Trl_3D.Core.Abstractions;

namespace Trl_3D.Core.Assertions
{
    public delegate void CaptureCallback(byte[] bufferOut, RenderInfo time);

    public class GrabScreenshot : IAssertion
    {
        /// <summary>
        /// Called onnce a screenshot is captured.
        /// </summary>
        public CaptureCallback CaptureCallback { get; set; }
    }
}
