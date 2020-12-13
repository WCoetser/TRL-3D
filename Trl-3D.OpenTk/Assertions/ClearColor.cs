using Trl_3D.Core.Abstractions;
using OpenTK.Graphics.OpenGL4;

namespace Trl_3D.Core.Assertions
{
    public class ClearColor : IAssertion
    {
        public float Red { get; }
        public float Green { get; }
        public float Blue { get; }

        public RenderProcessStep ProcessStep => RenderProcessStep.Start;

        public ClearColor(float red, float green, float blue)
        {
            Red = red;
            Green = green;
            Blue = blue;
        }

        public void SetState()
        {
            GL.ClearColor(Red, Green, Blue, 1.0f);
        }

        public void Render()
        {
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}
