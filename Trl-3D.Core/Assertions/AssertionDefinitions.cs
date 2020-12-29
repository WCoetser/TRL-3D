using Trl_3D.Core.Abstractions;

namespace Trl_3D.Core.Assertions
{
    // Note: These are in effect term definitions for making assertions about the world, ex. "The clear colour is ..."

    public record ClearColor(float Red, float Green, float Blue): IAssertion;

    public record GrabScreenshot() : IAssertion;    

    public record Triangle(ulong TriangleId, (ulong Point1Id, ulong Point2Id, ulong Point3Id) VertexIds) : IAssertion;

    public record ColorRgba(float Red, float Green, float Blue, float Opacity);

    public record Coordinate3d(float X, float Y, float Z);
}
