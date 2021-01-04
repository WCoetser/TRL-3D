using System;
using System.Threading.Tasks;

namespace Trl_3D.OpenTk.Textures
{
    public interface ITextureLoader
    {
        /// <summary>
        /// Loads texture to graphics card via OpenGL calls.
        /// </summary>
        Texture LoadTexture(Core.Scene.Texture texture);

        void Dispose();
    }
}
