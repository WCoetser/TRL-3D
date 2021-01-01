using Trl_3D.Core.Abstractions;

namespace Trl_3D.Core.Assertions
{
    // Note: These are in effect term definitions for making assertions about the world, ex. "The clear colour is ..."

    public record ClearColor(float Red, float Green, float Blue): IAssertion;

    public record GrabScreenshot() : IAssertion;    

    public record Triangle(ulong TriangleId, (ulong Point1Id, ulong Point2Id, ulong Point3Id) VertexIds) : IAssertion;

    public record Texture(ulong TextureId, string Uri) : IAssertion;

    public record TexCoords((ulong SurfaceId, ulong VertexId) ObjectIdentifier, ulong TextureId, float U, float V) : IAssertion;

    /// <summary>
    /// Colours are identified by a combination of a surface and vertex ID
    /// </summary>
    public record SurfaceColor((ulong SurfaceId, ulong VertexId) ObjectIdentifier, ColorRgba vertexColor) : IAssertion;

    /// <summary>
    /// Sets the view model matrix to "move the camera"
    /// The "up" vector is automatically calculated to get a camera that behaves like one in a first person shooter.
    /// </summary>
    public record CameraOrientation(Coordinate3d CameraLocation, Vector3d CameraDirection, Vector3d UpDirection) : IAssertion;

    /// <summary>
    /// Sets the projection used.
    /// Configures the projection matrix.
    /// </summary>
    public record CameraProjection() : IAssertion;
}
