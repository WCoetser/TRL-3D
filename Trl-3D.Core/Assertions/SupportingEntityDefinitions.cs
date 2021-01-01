using OpenTK.Mathematics;
using System;

namespace Trl_3D.Core.Assertions
{
    public record Coordinate3d(float X, float Y, float Z)
    {
        public Vector3 ToOpenTkVec3() => new Vector3(X,Y,Y);
    };

    public record Vector3d(float dX, float dY, float dZ)
    {
        public Vector3 ToOpenTkVec3() => new Vector3(dX, dY, dY);
    }

    public record ColorRgba(float Red, float Green, float Blue, float Opacity);
}
