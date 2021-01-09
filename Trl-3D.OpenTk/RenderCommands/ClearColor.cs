using Trl_3D.Core.Abstractions;

using OpenTK.Graphics.OpenGL4;
using Trl_3D.Core.Assertions;

namespace Trl_3D.OpenTk.RenderCommands
{
    public class ClearColor : IRenderCommand
    {
        public RenderProcessPosition ProcessStep => RenderProcessPosition.BeforeContent;

        private readonly ColorRgba _clearColor;

        public ClearColor(ColorRgba rgbColor)
        {
            _clearColor = rgbColor;
        }

        public bool SelfDestruct => false;

        public void Render(RenderInfo renderInfo)
        {
            GL.ClearColor(_clearColor.Red, _clearColor.Green, _clearColor.Blue, 1.0f);
            GL.Clear(ClearBufferMask.ColorBufferBit);
        }

        public void SetState(RenderInfo renderInfo)
        {
        }

        public PickingInfo RenderForPicking(RenderInfo renderInfo, int screenX, int screenY)
        {
            GL.ClearColor(0, 0, 0, 0);
            GL.Clear(ClearBufferMask.ColorBufferBit);
            return null;
        }
    }
}
