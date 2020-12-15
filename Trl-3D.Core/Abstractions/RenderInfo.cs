namespace Trl_3D.Core.Abstractions
{
    public class RenderInfo
    {
        public double TotalRenderTime { get; set; }

        public double FrameRate { get; set; }

        public int Width { get; set; }

        public int Height { get; set; }

        public RenderInfo Clone()
            => new RenderInfo
            {
                TotalRenderTime = TotalRenderTime,
                FrameRate = FrameRate,
                Height = Height,
                Width = Width
            };
    }
}
