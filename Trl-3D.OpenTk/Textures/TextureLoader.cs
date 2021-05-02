using OpenTK.Graphics.OpenGL4;
using System.Collections.Generic;
using System.Linq;

namespace Trl_3D.OpenTk.Textures
{
    public class TextureLoader : ITextureLoader
    {
        private readonly Dictionary<ulong, Texture> _knownTextures;

        public TextureLoader()
        {
            _knownTextures = new Dictionary<ulong, Texture>();
        }

        public Texture LoadTexture(Core.Scene.Texture texture)
        {
            if (_knownTextures.TryGetValue(texture.TextureId, out Texture textureOut))
            {
                return textureOut;
            }

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

            // TODO: Gen mipmaps, update min and mag filter

            textureOut = new Texture
            {
                ObjectId = texture.TextureId,
                OpenGLTextureId = textureIds[0]
            };

            _knownTextures.Add(texture.TextureId, textureOut);

            return textureOut;
        }

        public void Dispose()
        {
            // TODO: Fix transient "Attempted to read or write protected memory" error
            //var deleteList = _knownTextures.Values.Select(t => t.OpenGLTextureId).ToArray();
            //GL.DeleteTextures(deleteList.Length, deleteList);
        }
    }
}
