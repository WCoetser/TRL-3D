using OpenTK.Mathematics;

namespace Trl_3D.Core.Abstractions
{
    public class RenderInfo
    {
        public double TotalRenderTime { get; set; }

        public double FrameRate { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public Matrix4 CurrentViewMatrix { get; set; }

        public Matrix4 CurrentProjectionMatrix { get; set; }
    }
}
