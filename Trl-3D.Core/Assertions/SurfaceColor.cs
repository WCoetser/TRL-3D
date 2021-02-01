using System.Linq;
using Trl_3D.Core.Abstractions;

namespace Trl_3D.Core.Assertions
{
    /// <summary>
    /// Colours are identified by a combination of a surface and vertex ID
    /// </summary>
    public class SurfaceColor : ObjectIdentityBase, IAssertion
    {
        public SurfaceColor((ulong SurfaceId, ulong VertexId) ObjectIdentifier, ColorRgba VertexColor) 
            : base(ObjectIdentifier.SurfaceId, ObjectIdentifier.VertexId)
        {
            this.VertexColor = VertexColor;
        }

        public (ulong SurfaceId, ulong VertexId) ObjectIdentifier => (ObjectIds[0], ObjectIds[1]);

        public ColorRgba VertexColor { get; }
    }
}
