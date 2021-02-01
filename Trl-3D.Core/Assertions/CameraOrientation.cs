using OpenTK.Mathematics;
using Trl_3D.Core.Abstractions;

namespace Trl_3D.Core.Assertions
{
    /// <summary>
    /// Sets the view model matrix to "move the camera"
    /// The "up" vector is automatically calculated to get a camera that behaves like one in a first person shooter.
    /// </summary>
    public record CameraOrientation(Coordinate3d CameraLocation, Vector3d CameraDirection, Vector3d UpDirection) : IAssertion
    {
        public static CameraOrientation Default => new CameraOrientation(new(0f, 0f, 0f), new(0, 0, -1), new(0, 1, 0));

        public Matrix4 ToMatrix()
        {
            Vector3 eyePosition = new Vector3(CameraLocation.X, CameraLocation.Y, CameraLocation.Z);
            Vector3 eyeVector = new Vector3(CameraDirection.dX, CameraDirection.dY, CameraDirection.dZ);
            Vector3 upVector = new Vector3(UpDirection.dX, UpDirection.dY, UpDirection.dZ);
            var target = eyePosition + eyeVector;
            return Matrix4.LookAt(eyePosition, target, upVector);
        }
    }
}
