using Microsoft.Extensions.Logging;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using Trl_3D.Core.Assertions;

namespace Trl_3D.OpenTk
{
    public class OpenGLSceneProcessor
    {
        // References: 
        // https://learnopengl.com/Getting-started/Hello-Triangle
        // http://dreamstatecoding.blogspot.com/2017/02/opengl-4-with-opentk-in-c-part-5.html

        private readonly ILogger _logger;
        
        private int _program;
        private int _vertexArrayObject;
        private int _vertexBufferObject;
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

        public OpenGLSceneProcessor(ILogger logger)
        {
            _logger = logger;
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

        internal void ResizeRenderWindow(int width, int height)
        {
            GL.Viewport(0, 0, width, height);
            _logger.LogInformation($"Window resized to {width}x{height}={width*height} pixels");
        }

        public void SetState(IEnumerable<Core.Abstractions.IAssertion> scene)
        {
            _program = CompileShaders();

            CreateBuffers();

            foreach (var assertion in scene)
            {
                if (assertion is ClearColor clearColor)
                {
                    // Process
                    GL.ClearColor(clearColor.Red, clearColor.Green, clearColor.Blue, 1.0f);
                    continue;
                }

                throw new Exception($"Unknown assertion type: {assertion.GetType().FullName}");
            }
        }

        private void CreateBuffers()
        {
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

        public void Render(double time)
        {
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            GL.UseProgram(_program);
            GL.BindVertexArray(_vertexArrayObject);
            GL.DrawArrays(PrimitiveType.Triangles, 0, 3);
        }

        public void ReleaseResources()
        {
            GL.DeleteProgram(_program);
            GL.DeleteVertexArrays(1, ref _vertexArrayObject);
            GL.DeleteBuffer(_vertexBufferObject);            
        }
    }
}
