using Trl_3D.Core.Abstractions;

namespace Trl_3D.OpenTk.GeometryBuffers
{
    public interface IGeometryBuffer
    {
        public void SetState(bool isReload);
        public void Render(RenderInfo info);
        public void Reload();
    }
}
