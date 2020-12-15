using Trl_3D.Core.Abstractions;
using OpenTK.Graphics.OpenGL4;
using Microsoft.Extensions.Logging;

namespace Trl_3D.Core.Assertions
{
    /// <summary>
    /// Tutorial References:
    /// https://dreamstatecoding.blogspot.com/p/opengl4-with-opentk-tutorials.html
    /// https://learnopengl.com/Getting-started/Hello-Triangle
    /// </summary>
    public class RenderTestTriagle : IAssertion
    {
        public RenderProcessStep ProcessStep => RenderProcessStep.Middle;

        public bool SelfDestruct => false;

        private readonly ILogger _logger;

        private int _vertexArrayObject;
        private int _vertexBufferObject;

        public RenderTestTriagle(ILogger logger)
        {
            _logger = logger;
        }

        #region Shaders

        private int _program;
        const string vertexShaderCode =
@"
#version 450 core

layout (location = 0) in vec3 aPos;

void main()
{
    gl_Position = vec4(aPos.x, aPos.y, aPos.z, 1.0);
}";

        const string fragmentShaderCode =
@"
#version 450 core

out vec4 FragColor;

void main()
{
    FragColor = vec4(1.0f, 0.5f, 0.2f, 1.0f);
} 
";

        #endregion

        public void Render(RenderInfo info)
        {
            GL.UseProgram(_program);
            GL.BindVertexArray(_vertexArrayObject);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
        }

        public void SetState()
        {
            _program = CompileShaders();

            var vertices = new[] {
                -0.5f, -0.5f, 0.0f,
                 0.5f, -0.5f, 0.0f,
                 0.0f,  0.5f, 0.0f
            };
            var stride = 3 * sizeof(float); // 12 bytes per row = data for one vertex

            // Draw data
            _vertexBufferObject = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticCopy);

            // Draw attributes
            _vertexArrayObject = GL.GenVertexArray();
            GL.BindVertexArray(_vertexArrayObject);
            GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
            GL.BufferData(BufferTarget.ArrayBuffer, vertices.Length * sizeof(float), vertices, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
            GL.EnableVertexAttribArray(0);
        }

        private int CompileShaders()
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

            var program = GL.CreateProgram();
            GL.AttachShader(program, vertexShader);
            GL.AttachShader(program, fragmentShader);
            GL.LinkProgram(program);

            info = GL.GetProgramInfoLog(program);
            if (!string.IsNullOrWhiteSpace(info))
                _logger.LogError($"Shared linking information: {info}");

            GL.DetachShader(program, vertexShader);
            GL.DetachShader(program, fragmentShader);
            GL.DeleteShader(vertexShader);
            GL.DeleteShader(fragmentShader);
            return program;
        }

        public void Dispose()
        {
            GL.DeleteProgram(_program);
            GL.DeleteVertexArrays(1, ref _vertexArrayObject);
            GL.DeleteBuffer(_vertexBufferObject);
        }
    }
}
