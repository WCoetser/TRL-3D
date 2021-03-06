﻿using OpenTK.Mathematics;

namespace Trl_3D.Core.Abstractions
{
    /// <summary>
    /// Returns the ID of the object/surface and location for the given screen and time coordinates.
    /// </summary>
    public record PickingInfo(ulong? ObjectId, double Time, int ScreenX, int ScreenY, Vector3 WorldSpaceCoordinates);
}
