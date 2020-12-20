using System;
using Trl_3D.Core.Abstractions;

namespace Trl_3D.Core.Assertions
{
    public class GrabScreenshot : IAssertion
    {
        /// <summary>
        /// Called onnce a screenshot is captured.
        /// </summary>
        public Action<byte[], int, int> CaptureCallback { get; set; } // buffer, width, height
    }
}
