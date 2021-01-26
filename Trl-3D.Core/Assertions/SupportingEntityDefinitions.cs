using OpenTK.Mathematics;

namespace Trl_3D.Core.Assertions
{
    public record Coordinate3d(float X, float Y, float Z)
    {
        public Vector3 ToOpenTkVec3() => new Vector3(X,Y,Z);

        public Coordinate3d Transform(Matrix4 tranformationMatrix)
        {
            var v = new Vector4(X, Y, Z, 1.0f);
            var newPosition = tranformationMatrix * v;
            return new Coordinate3d(newPosition.X, newPosition.Y, newPosition.Z);
        }
    };

    public record Vector3d(float dX, float dY, float dZ)
    {
        public Vector3 ToOpenTkVec3() => new Vector3(dX, dY, dY);
    }

    public record ColorRgba(float Red, float Green, float Blue, float Opacity);
}
