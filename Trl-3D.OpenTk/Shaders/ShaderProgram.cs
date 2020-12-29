using System;
using OpenTK.Graphics.OpenGL4;

namespace Trl_3D.OpenTk.Shaders
{
    public class ShaderProgram : IDisposable
    {
        public int ProgramId { get; }

        public ShaderProgram(int programId)
        {
            ProgramId = programId;
        }

        public void Dispose()
        {
            GL.DeleteProgram(ProgramId);
        }
    }
}
