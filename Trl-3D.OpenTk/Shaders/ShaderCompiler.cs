using Microsoft.Extensions.Logging;
using OpenTK.Graphics.OpenGL4;

namespace Trl_3D.OpenTk.Shaders
{
    public class ShaderCompiler : IShaderCompiler
    {
        private readonly ILogger<ShaderCompiler> _logger;

        public ShaderCompiler(ILogger<ShaderCompiler> logger)
        {
            _logger = logger;
        }

        public ShaderProgram Compile(string vertexShaderCode, string fragmentShaderCode)
        {
            var vertexShader = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertexShader, vertexShaderCode);
            GL.CompileShader(vertexShader);

            var info = GL.GetShaderInfoLog(vertexShader);
            if (!string.IsNullOrWhiteSpace(info))
                _logger.LogError($"Vertex shader compilation: {info}");

            var fragmentShader = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragmentShader, fragmentShaderCode);
            GL.CompileShader(fragmentShader);

            info = GL.GetShaderInfoLog(fragmentShader);
            if (!string.IsNullOrWhiteSpace(info))
                _logger.LogError($"Vertex shader compilation: {info}");

            var programId = GL.CreateProgram();
            GL.AttachShader(programId, vertexShader);
            GL.AttachShader(programId, fragmentShader);
            GL.LinkProgram(programId);

            info = GL.GetProgramInfoLog(programId);
            if (!string.IsNullOrWhiteSpace(info))
                _logger.LogError($"Shared linking information: {info}");

            GL.DetachShader(programId, vertexShader);
            GL.DetachShader(programId, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);

            return new ShaderProgram(programId);
        }
    }
}
