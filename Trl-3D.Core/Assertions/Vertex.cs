using Trl_3D.Core.Abstractions;

namespace Trl_3D.Core.Assertions
{
    public class Vertex : IAssertion
    {
        public ulong VertexId { get; }
        public ColorRgba Color { get; }
        public Coordinate3d Coordinates { get; }

        public Vertex(ulong vertexId, Coordinate3d coordinates)
        {
            VertexId = vertexId;
            Coordinates = coordinates;
        }

        public Vertex(ulong vertexId, ColorRgba color)
        {
            VertexId = vertexId;
            Color = color;
        }
    }
}
