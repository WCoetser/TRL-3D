using System;
using Trl_3D.Core.Abstractions;
using Trl_3D.Core.Assertions;

namespace Trl_3D.Core.Scene
{
    public class AssertionProcessor
    {
        /// <summary>
        /// Updates the scenegraph with the information from the assertion.
        /// </summary>
        public void Process(IAssertion assertion, SceneGraph sceneGraph)
        {
            if (assertion is ClearColor clearColor)
            {
                sceneGraph.RgbClearColor = new[] { clearColor.Red, clearColor.Green, clearColor.Blue };
            }
            else if (assertion is RenderTestTriagle renderTestTriagle)
            {
                // TODO: load surfaces and vertices
            }
            else if (assertion is GrabScreenshot grabScreenshot)
            {
                // Nothing to do here, screenshots passed out to event processor via render window event channel
            }
            else
            {
                throw new ArgumentException("Unknown assertion type");
            }
        }
    }
}
