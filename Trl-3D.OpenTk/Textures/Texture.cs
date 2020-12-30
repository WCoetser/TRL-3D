using System;
using OpenTK.Graphics.OpenGL4;

namespace Trl_3D.OpenTk.Textures
{
    public class Texture : IDisposable
    {
        public ulong ObjectId { get; init; }
        public uint OpenGLTextureId { get; init; }

        public void Dispose()
        {
            GL.DeleteTextures(1, new[] { OpenGLTextureId });
        }
    }
}
