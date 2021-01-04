using System;
using OpenTK.Graphics.OpenGL4;

namespace Trl_3D.OpenTk.Textures
{
    public class Texture
    {
        public ulong ObjectId { get; init; }
        public uint OpenGLTextureId { get; init; }
    }
}
