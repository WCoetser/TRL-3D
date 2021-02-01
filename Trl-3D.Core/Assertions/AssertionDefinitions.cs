using Trl_3D.Core.Abstractions;

namespace Trl_3D.Core.Assertions
{
    public record ClearColor(float Red, float Green, float Blue): IAssertion;

    public record GrabScreenshot() : IAssertion;    
    
    /// <summary>
    /// Sets the projection used.
    /// Configures the projection matrix.
    /// </summary>
    public record CameraProjectionPerspective(float FieldOfViewVerticalDegrees, float NearPlane, float FarPlane) : IAssertion;

    /// <summary>
    /// Gets information about an object rendered at the given coordinates.
    /// </summary>
    public record GetPickingInfo(int ScreenX, int ScreenY) : IAssertion;
}
