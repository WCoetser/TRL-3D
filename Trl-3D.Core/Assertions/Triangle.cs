using Trl_3D.Core.Abstractions;

namespace Trl_3D.Core.Assertions
{
    public class Triangle : ObjectIdentityBase, IAssertion
    {
        public Triangle(ulong triangleId, (ulong Point1Id, ulong Point2Id, ulong Point3Id) vertexIds) : base(triangleId)
        {
            TriangleId = triangleId;
            VertexIds = vertexIds;
        }

        public ulong TriangleId { get; }
        public (ulong Point1Id, ulong Point2Id, ulong Point3Id) VertexIds { get; }
    }
}
