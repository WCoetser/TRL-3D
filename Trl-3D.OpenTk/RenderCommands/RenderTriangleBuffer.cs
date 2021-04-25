using Trl_3D.Core.Abstractions;
using Trl_3D.OpenTk.GeometryBuffers;

namespace Trl_3D.OpenTk.RenderCommands
{
    public class RenderTriangleBuffer : IRenderCommand
    {
        public RenderProcessPosition ProcessStep => RenderProcessPosition.ContentRenderStep;

        public bool SelfDestruct => false;

        private readonly TriangleBuffer _triangleBuffer;

        internal RenderTriangleBuffer(TriangleBuffer triangleBuffer)
        {
            _triangleBuffer = triangleBuffer;
        }

        public void Render(RenderInfo renderInfo)
        {
            _triangleBuffer.Render(renderInfo);
        }

        public void SetState(RenderInfo renderInfo)
        {
            _triangleBuffer.SetState(false);
        }

        public void Dispose()
        {
            _triangleBuffer.Dispose();
        }
    }
}

