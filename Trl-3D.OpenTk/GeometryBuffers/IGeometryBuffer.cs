using Trl_3D.Core.Abstractions;

namespace Trl_3D.OpenTk.GeometryBuffers
{
    public interface IGeometryBuffer
    {
        public void SetState();
        public void Render(RenderInfo info);
        public PickingInfo RenderForPicking(RenderInfo info, int screenX, int screenY);
    }
}
