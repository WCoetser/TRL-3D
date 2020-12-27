using Trl_3D.Core.Abstractions;

using OpenTK.Graphics.OpenGL4;

using Microsoft.Extensions.Logging;
using Trl_3D.Core.Scene;
using System.Linq;

namespace Trl_3D.OpenTk.RenderCommands
{
    public class RenderSceneGraph : IRenderCommand
    {
        public RenderProcessPosition ProcessStep => RenderProcessPosition.ContentRenderStep;

        public bool SelfDestruct => false;

        private readonly ILogger _logger;
        private readonly SceneGraph _sceneGraph;
        private int _vertexArrayObject;
        private int _vertexBufferObject;

        public RenderSceneGraph(ILogger logger, SceneGraph sceneGraph)
        {
            _logger = logger;
            _sceneGraph = sceneGraph;
        }

        #region Shaders

        private int _program;
        private int _triangleCount;
        const string vertexShaderCode =
@"
#version 450 core

layout (location = 0) in vec3 vertexPosition;

void main()
{
    gl_Position = vec4(vertexPosition.x, vertexPosition.y, vertexPosition.z, 1.0);
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
            GL.DrawArrays(PrimitiveType.Triangles, 0, _triangleCount * 3);
        }

        public void SetState()
        {   
            _program = CompileShaders();

            // Load triangles into render buffer for batch rendering
            const int componentsPerVertex = 3; // only 3D location for now
            const int verticesPerTriangle = 3;
            var readyListCount = _sceneGraph.GetCompleteTriangles().Count();
            var vertexBuffer = new float[readyListCount * componentsPerVertex * verticesPerTriangle];
            int position = 0;
            _triangleCount = 0;
            foreach (var triangle in _sceneGraph.GetCompleteTriangles())
            {
                void loadVertexPosition(Vertex v)
                {
                    vertexBuffer[position++] = v.Coordinates.X;
                    vertexBuffer[position++] = v.Coordinates.Y;
                    vertexBuffer[position++] = v.Coordinates.Z;
                };
                
                loadVertexPosition(triangle.v1);
                loadVertexPosition(triangle.v2);
                loadVertexPosition(triangle.v3);

                _triangleCount++;
            }

            var stride = componentsPerVertex * sizeof(float); // 12 bytes per row = data for one vertex position

            var vertexArrays = new int[1];
            GL.CreateVertexArrays(1, vertexArrays);
            GL.BindVertexArray(vertexArrays[0]);
            _vertexArrayObject = vertexArrays[0];

            var buffers = new int[1];
            GL.CreateBuffers(1, buffers);
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffers[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, vertexBuffer.Length * sizeof(float), vertexBuffer, BufferUsageHint.StaticCopy);
            _vertexBufferObject = buffers[0];

            GL.EnableVertexArrayAttrib(buffers[0], 0);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, stride, 0);
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

