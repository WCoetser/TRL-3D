namespace Trl_3D.Core.Scene
{
    public class Texture : SceneGraphObjectBase
    {
        public Texture(SceneGraph sceneGraph, ulong objectId, byte[] imageDataRgba, int width, int height) : base(sceneGraph, objectId)
        {
            ImageDataRgba = imageDataRgba;
            Width = width;
            Height = height;
        }

        public int Height { get; }

        public int Width { get; }

        public byte[] ImageDataRgba { get; }
    }
}
