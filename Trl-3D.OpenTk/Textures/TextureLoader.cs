using OpenTK.Graphics.OpenGL4;

namespace Trl_3D.OpenTk.Textures
{
    public class TextureLoader : ITextureLoader
    {
        public Texture LoadTexture(Core.Scene.Texture texture)
        {
            var textureIds = new uint[1];
            GL.GenTextures(1, textureIds);
            GL.BindTexture(TextureTarget.Texture2D, textureIds[0]);

            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, texture.Width, texture.Height, 
                0, PixelFormat.Rgba, PixelType.UnsignedByte, texture.ImageDataRgba);

            GL.BindTexture(TextureTarget.Texture2D, 0);

            return new Texture {
                ObjectId = texture.ObjectId,
                OpenGLTextureId = textureIds[0]                
            };
        }
    }
}
