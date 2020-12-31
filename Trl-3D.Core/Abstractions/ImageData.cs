namespace Trl_3D.Core.Abstractions
{
    /// <summary>
    /// Pixels are loaded per column.
    /// </summary>
    public record ImageData(byte[] BufferRgba, int Width, int Height);
}
