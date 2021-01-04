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
        }

        public void SetState(RenderInfo renderInfo)
        {
            GL.ClearColor(_clearColor.Red, _clearColor.Green, _clearColor.Blue, 1.0f);
        }
    }
}
