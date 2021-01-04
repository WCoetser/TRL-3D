using Trl_3D.Core.Assertions;

namespace Trl_3D.SampleApp
{
    public static class Constants
    {
        public static CameraOrientation DefaultCameraOrientation => new CameraOrientation(new(0f, -0.15f, 1f), new(0, 0, -1), new(0, 1, 0));
    }
}
