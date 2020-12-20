using Trl_3D.Core.Abstractions;

using OpenTK.Graphics.OpenGL4;

namespace Trl_3D.OpenTk.RenderCommands
{
    public class ClearColor : IRenderCommand
    {
        public RenderProcessPosition ProcessStep => RenderProcessPosition.BeforeContent;

        private readonly float[] _clearColor;

        public ClearColor(float[] rgbColor)
        {
            _clearColor = rgbColor;
        }

        public bool SelfDestruct => false;

        public void Dispose()
        {
            // Nothing to dispose
        }

        public void Render(RenderInfo renderInfo)
        {
            // TODO: Refactor depth buffer out
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
        }

        public void SetState()
        {
            GL.ClearColor(_clearColor[0], _clearColor[1], _clearColor[2], 1.0f);
        }
    }
}
