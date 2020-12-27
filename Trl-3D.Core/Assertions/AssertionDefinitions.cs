using Trl_3D.Core.Abstractions;
using Trl_3D.Core.Scene;

namespace Trl_3D.Core.Assertions
{
    public record ClearColor(float Red, float Green, float Blue): IAssertion;

    public record GrabScreenshot() : IAssertion;

    public record Vertex(ulong vertexId, Coordinate3d Coordinates) : IAssertion;

    public record Triangle(ulong triangleId, (ulong Point1Id, ulong Point2Id, ulong Point3Id) VertexIds) : IAssertion;
}
