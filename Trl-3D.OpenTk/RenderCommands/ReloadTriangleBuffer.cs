using Trl_3D.Core.Abstractions;
using Trl_3D.OpenTk.GeometryBuffers;

namespace Trl_3D.OpenTk.RenderCommands
{
    internal class ReloadTriangleBuffer : IRenderCommand
    {
        private readonly TriangleBuffer _triangleBuffer;

        public ReloadTriangleBuffer(TriangleBuffer triangleBuffer)
        {
            _triangleBuffer = triangleBuffer;
        }
        public RenderProcessPosition ProcessStep => RenderProcessPosition.BeforeContent;

        public bool SelfDestruct => true;

        public void Render(RenderInfo renderInfo)
        {
            // nothing to show here ... the buffer should already have a command
        }

        public PickingInfo RenderForPicking(RenderInfo renderInfo, int screenX, int screenY)
        {
            // nothing to show here ... the buffer should already have a command
            return null;
        }

        public void SetState(RenderInfo renderInfo)
        {
            _triangleBuffer.Reload();
        }
    }
}
