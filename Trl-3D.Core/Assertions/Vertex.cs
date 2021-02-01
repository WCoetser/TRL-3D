using Trl_3D.Core.Abstractions;

namespace Trl_3D.Core.Assertions
{
    public class Vertex : ObjectIdentityBase, IAssertion
    {
        public SurfaceColor Color { get; }
        public Coordinate3d Coordinates { get; }

        public Vertex(ulong vertexId, Coordinate3d coordinates) : base(vertexId)
        {
            Coordinates = coordinates;
        }

        public ulong VertexId => ObjectIds[0];
    }
}
