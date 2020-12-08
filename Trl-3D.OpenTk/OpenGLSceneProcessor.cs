using OpenTK.Graphics.ES30;
using System.Collections.Generic;
using Trl_3D.Core.Assertions;

namespace Trl_3D.OpenTk
{
    public class OpenGLSceneProcessor
    {
        public void SetState(IEnumerable<Core.Abstractions.IAssertion> scene)
        {
            foreach (var assertion in scene)
            {
                if (assertion is ClearColor clearColor)
                {
                    // Process
                    GL.ClearColor(clearColor.Red, clearColor.Green, clearColor.Blue, 1.0f);
                }
            }
        }

        public void Render()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }
    }
}
