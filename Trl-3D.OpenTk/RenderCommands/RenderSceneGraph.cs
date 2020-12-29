using Trl_3D.Core.Abstractions;

using OpenTK.Graphics.OpenGL4;

using Microsoft.Extensions.Logging;
using Trl_3D.Core.Scene;
using System.Collections.Generic;
using Trl_3D.OpenTk.Shaders;
using System.Buffers;
using System;

namespace Trl_3D.OpenTk.RenderCommands
{
    public class RenderSceneGraph : IRenderCommand
    {
        public RenderProcessPosition ProcessStep => RenderProcessPosition.ContentRenderStep;

        public bool SelfDestruct => false;

        private readonly ILogger _logger;
        private readonly SceneGraph _sceneGraph;
        private readonly IShaderCompiler _shaderCompiler;
        private int _vertexArrayObject;
        private int _vertexBufferObject;
        private int _vertexIndexBuffer;
        private int _triangleCount;

        public RenderSceneGraph(ILogger logger, IShaderCompiler shaderCompiler, SceneGraph sceneGraph)
        {
            _logger = logger;
            _sceneGraph = sceneGraph;
            _shaderCompiler = shaderCompiler;
        }

        #region Shaders

        private ShaderProgram _shaderProgram;

        const string vertexShaderCode =
@"
#version 450 core

layout (location = 0) in float vertexIdIn;
layout (location = 1) in float surfaceIdIn;
layout (location = 2) in vec3 vertexPosition;
layout (location = 3) in vec4 vertexColorIn;

out int vertexId;
out int surfaceId;
out vec4 vertexColor;

void main()
{
    vertexId = int(vertexIdIn);
    surfaceId = int(surfaceIdIn);
    vertexColor = vertexColorIn;

    gl_Position = vec4(vertexPosition.x, vertexPosition.y, vertexPosition.z, 1.0);
}";

        const string fragmentShaderCode =
@"
#version 450 core

in int vertexId;
in int surfaceId;
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
            GL.UseProgram(_shaderProgram.ProgramId);
            GL.BindVertexArray(_vertexArrayObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _vertexIndexBuffer);
            GL.DrawElements(PrimitiveType.Triangles, _triangleCount * 3,DrawElementsType.UnsignedInt, 0);
        }

        public void SetState()
        {   
            _shaderProgram = _shaderCompiler.Compile(vertexShaderCode, fragmentShaderCode);
                                 
            Dictionary<ulong, uint> vertexIdToIndex = new Dictionary<ulong, uint>();            
            var vertexIndexBufferWriter = new ArrayBufferWriter<uint>();
            var vertexBufferWriter = new ArrayBufferWriter<float>();

            foreach (var triangle in _sceneGraph.GetCompleteTriangles())
            {
                var vertices = triangle.GetVertices();

                uint loadVertexPosition(Vertex v)
                {
                    if (vertexIdToIndex.TryGetValue(v.ObjectId, out uint vertexIndex))
                    {
                        return vertexIndex;
                    }

                    var vertexComponents = new ReadOnlySpan<float>(new float[]
                    {
                        // Object IDs
                        v.ObjectId,
                        triangle.ObjectId,
                        // Coordinates
                        v.Coordinates.X,
                        v.Coordinates.Y,
                        v.Coordinates.Z,
                        // Colour
                        v.Color?.Red ?? 1.0f,
                        v.Color?.Green ?? 1.0f,
                        v.Color?.Blue ?? 1.0f,
                        v.Color?.Opacity ?? 1.0f
                    });
                    vertexBufferWriter.Write(vertexComponents);

                    var retIndex = (uint)vertexIdToIndex.Count;
                    vertexIdToIndex[v.ObjectId] = retIndex;
                    return retIndex;
                };

                var indices = new ReadOnlySpan<uint>(new uint[]
                {
                    loadVertexPosition(vertices.Item1),
                    loadVertexPosition(vertices.Item2),
                    loadVertexPosition(vertices.Item3)
                });
                vertexIndexBufferWriter.Write(indices);

                _triangleCount++;
            }

            const int componentsPerVertex = 9; // 3D location + vertex ID + surface ID + 4 component colour
            var stride = componentsPerVertex * sizeof(float);

            var vertexBuffer = vertexBufferWriter.WrittenMemory.ToArray();

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
            GL.VertexAttribPointer(layout_pos_vertexPosition, 3, VertexAttribPointerType.Float, false, stride, 2 * sizeof(float));

            // Vertex colour
            const int layout_pos_vertexColor = 3;
            GL.EnableVertexArrayAttrib(buffers[0], layout_pos_vertexColor);
            GL.VertexAttribPointer(layout_pos_vertexColor, 4, VertexAttribPointerType.Float, false, stride, 5 * sizeof(float));

            // Vertex index buffer for triangles    
            var vertexIndexBuffer = vertexIndexBufferWriter.WrittenMemory.ToArray();
            var indexBuffers = new int[1];
            GL.CreateBuffers(1, indexBuffers);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffers[0]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, vertexIndexBuffer.Length * sizeof(uint), vertexIndexBuffer, BufferUsageHint.StaticDraw);
            _vertexIndexBuffer = indexBuffers[0];

            // Log stats
            _logger.LogInformation($"Vertex buffer size = {vertexBuffer.Length * sizeof(float)} bytes");
            _logger.LogInformation($"Vertex index buffer size = {vertexIndexBuffer.Length * sizeof(uint)} bytes");
            if (_triangleCount != _sceneGraph.Triangles.Count)
            {
                _logger.LogWarning($"Some triangles have missing vertices and will not be rendered.");
            }
        }

        public void Dispose()
        {
            _shaderProgram?.Dispose();
            GL.DeleteVertexArrays(1, ref _vertexArrayObject);
            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteBuffer(_vertexIndexBuffer);
        }
    }
}

