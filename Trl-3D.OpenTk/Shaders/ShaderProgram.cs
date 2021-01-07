using System;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace Trl_3D.OpenTk.Shaders
{
    public class ShaderProgram : IDisposable
    {
        public int ProgramId { get; }

        public ShaderProgram(int programId)
        {
            ProgramId = programId;
        }

        public void UseProgram() => GL.UseProgram(ProgramId);

        public void SetUniform(string name, Matrix4 matrix)
        {
            var matrixLocation = GL.GetUniformLocation(ProgramId, name);
            GL.UniformMatrix4(matrixLocation, false, ref matrix);
        }

        public void SetUniform(string name, int[] values)
        {
            var samplerArrayLocation = GL.GetUniformLocation(ProgramId, name);
            GL.Uniform1(samplerArrayLocation, values.Length, values);
        }

        public void Dispose()
        {
            GL.DeleteProgram(ProgramId);
        }
    }
}
