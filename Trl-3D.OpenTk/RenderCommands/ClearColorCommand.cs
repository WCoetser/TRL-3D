using Trl_3D.Core.Abstractions;
using Trl_3D.Core.Assertions;

using OpenTK.Graphics.OpenGL4;
using System;

namespace Trl_3D.OpenTk.RenderCommands
{
    public class ClearColorCommand : IRenderCommand
    {
        public RenderProcessPosition ProcessStep => RenderProcessPosition.BeforeContent;

        public bool SelfDestruct => false;

        public Type AssociatedAssertionType => typeof(ClearColor);

        public void Dispose()
        {
            // Nothing to dispose
        }

        public void Render(RenderInfo renderInfo)
        {
            // TODO: Refactor depth buffer out
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public void SetState(IAssertion assertion)
        {
            var clearColor = (ClearColor)assertion;
            GL.ClearColor(clearColor.Red, clearColor.Green, clearColor.Blue, 1.0f);
        }
    }
}
