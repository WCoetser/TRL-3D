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

layout (location = 0) in float vertexIdIn;
layout (location = 1) in float surfaceIdIn;
layout (location = 2) in vec3 vertexPosition;
layout (location = 3) in vec4 vertexColorIn;

out float vertexId;
out float surfaceId;
out vec4 vertexColor;

void main()
{
    vertexId = vertexIdIn;
    surfaceId = surfaceIdIn;
    vertexColor = vertexColorIn;

    gl_Position = vec4(vertexPosition.x, vertexPosition.y, vertexPosition.z, 1.0);
}";

        const string fragmentShaderCode =
@"
#version 450 core

in float vertexId;
in float surfaceId;
in vec4 vertexColor;

out vec4 pixelColorOut;

void main()
{    
    pixelColorOut = vertexColor;
} 
";

        #endregion

        public void Render(RenderInfo info)
        {
            GL.UseProgram(_program);
            GL.BindVertexArray(_vertexArrayObject);
            GL.DrawArrays(PrimitiveType.Triangles, 0, _triangleCount * 3);
        }

        public void CheckFloatValue(ulong vIn)
        {
            float f = vIn;
            if ((ulong)f != vIn)
            {
                // TODO
                throw new System.Exception($"Unable to represent vertex ID {vIn} as float");
            }
        }

        public void SetState()
        {   
            _program = CompileShaders();

            // Load triangles into render buffer for batch rendering
            const int componentsPerVertex = 9; // 3D location + vertex ID + surface ID + 4 component colour
            const int verticesPerTriangle = 3;
            var readyListCount = _sceneGraph.GetCompleteTriangles().Count();
            var vertexBuffer = new float[readyListCount * componentsPerVertex * verticesPerTriangle];
            int position = 0;
            _triangleCount = 0;

            foreach (var triangle in _sceneGraph.GetCompleteTriangles())
            {
                var vertices = triangle.GetVertices();

                void loadVertexPosition(Vertex v)
                {
                    CheckFloatValue(v.ObjectId);
                    CheckFloatValue(triangle.ObjectId);

                    float vertexId = v.ObjectId;
                    float surfaceId = triangle.ObjectId;

                    vertexBuffer[position++] = vertexId;
                    vertexBuffer[position++] = surfaceId;

                    vertexBuffer[position++] = v.Coordinates.X;
                    vertexBuffer[position++] = v.Coordinates.Y;
                    vertexBuffer[position++] = v.Coordinates.Z;

                    vertexBuffer[position++] = v.Color?.Red ?? 1.0f;
                    vertexBuffer[position++] = v.Color?.Green ?? 1.0f;
                    vertexBuffer[position++] = v.Color?.Blue ?? 1.0f;
                    vertexBuffer[position++] = v.Color?.Opacity ?? 1.0f;
                };
                
                loadVertexPosition(vertices.Item1);
                loadVertexPosition(vertices.Item2);
                loadVertexPosition(vertices.Item3);

                _triangleCount++;
            }

            var stride = componentsPerVertex * sizeof(float);

            var vertexArrays = new int[1];
            GL.CreateVertexArrays(1, vertexArrays);
            GL.BindVertexArray(vertexArrays[0]);
            _vertexArrayObject = vertexArrays[0];

            var buffers = new int[1];
            GL.CreateBuffers(1, buffers);
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffers[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, vertexBuffer.Length * sizeof(float), vertexBuffer, BufferUsageHint.StaticCopy);
            _vertexBufferObject = buffers[0];

            // Vertex ID
            const int layout_pos_vertexId = 0;
            GL.EnableVertexArrayAttrib(buffers[0], layout_pos_vertexId);
            GL.VertexAttribPointer(layout_pos_vertexId, 3, VertexAttribPointerType.Float, false, stride, 0);

            // Surface ID
            const int layout_pos_surfaceId = 1;
            GL.EnableVertexArrayAttrib(buffers[0], layout_pos_surfaceId);
            GL.VertexAttribPointer(layout_pos_surfaceId, 3, VertexAttribPointerType.Float, false, stride, sizeof(float));

            // Vertex position
            const int layout_pos_vertexPosition = 2;
            GL.EnableVertexArrayAttrib(buffers[0], layout_pos_vertexPosition);
            GL.VertexAttribPointer(layout_pos_vertexPosition, 3, VertexAttribPointerType.Float, false, stride, 2* sizeof(float));

            // Vertex colour
            const int layout_pos_vertexColor = 3;
            GL.EnableVertexArrayAttrib(buffers[0], layout_pos_vertexColor);
            GL.VertexAttribPointer(layout_pos_vertexColor, 4, VertexAttribPointerType.Float, false, stride, 5 * sizeof(float));
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

