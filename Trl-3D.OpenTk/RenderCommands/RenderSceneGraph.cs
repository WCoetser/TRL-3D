using OpenTK.Graphics.OpenGL4;

using Microsoft.Extensions.Logging;

using System.Buffers;
using System;
using System.Linq;
using System.Collections.Generic;

using Trl.IntegerMapper.EqualityComparerIntegerMapper;
using Trl.IntegerMapper;

using Trl_3D.Core.Abstractions;
using Trl_3D.Core.Scene;
using Trl_3D.OpenTk.Textures;
using Trl_3D.Core.Assertions;
using Trl_3D.OpenTk.Shaders;
using OpenTK.Mathematics;

namespace Trl_3D.OpenTk.RenderCommands
{
    public class RenderSceneGraph : IRenderCommand
    {
        public RenderProcessPosition ProcessStep => RenderProcessPosition.ContentRenderStep;

        public bool SelfDestruct => false;

        private readonly ILogger _logger;
        private readonly SceneGraph _sceneGraph;
        private readonly IShaderCompiler _shaderCompiler;
        private readonly ITextureLoader _textureLoader;

        private List<Textures.Texture> _activeTextures;

        // TODO: Make this per surface
        //private object ModelMatrix;
        
        // TODO: Move view and projection matrices to main processing loop ...
        //private object ProjectionMatrix;

        private int _vertexArrayObject;
        private int _vertexBufferObject;
        private int _vertexIndexBuffer;
        private int _triangleCount;
        private int _maxFragmentShaderTextureUnits;

        public RenderSceneGraph(ILogger logger, IShaderCompiler shaderCompiler, ITextureLoader textureLoader, SceneGraph sceneGraph)
        {
            _logger = logger;
            _sceneGraph = sceneGraph;
            _shaderCompiler = shaderCompiler;
            _textureLoader = textureLoader;
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
layout (location = 4) in vec2 texCoordsIn;
layout (location = 5) in float samplerIndexIn;

out float vertexId;
out float surfaceId;
out vec4 vertexColor;
out vec2 texCoords;
out float samplerIndex;

uniform mat4 viewMatrix;

void main()
{
    vertexId = vertexIdIn;
    surfaceId = surfaceIdIn;
    vertexColor = vertexColorIn;
    texCoords = texCoordsIn;
    samplerIndex = samplerIndexIn;

    gl_Position = viewMatrix * vec4(vertexPosition.x, vertexPosition.y, vertexPosition.z, 1.0);
}";

        string GetFragmentShaderCode(int maxFragmentShaderTextureUnits)
        {
            return $@"
#version 450 core

in float vertexId;
in float surfaceId;
in vec4 vertexColor;
in vec2 texCoords;
in float samplerIndex;

uniform sampler2D samplers[{maxFragmentShaderTextureUnits}];

out vec4 pixelColorOut;

void main() {{
    int intSamplerIndex = int(samplerIndex);
    if (intSamplerIndex != -1) {{
        pixelColorOut = texture(samplers[intSamplerIndex], texCoords);
    }}
    else {{
        pixelColorOut = vertexColor;
    }}
}}
";
        }

        #endregion

        public void Render(RenderInfo info)
        {
            for (int i = 0; i < _activeTextures.Count; i++)
            {
                GL.BindTextureUnit((uint)i, _activeTextures[i].OpenGLTextureId);
            }

            GL.UseProgram(_shaderProgram.ProgramId);

            // Set the view matrix for world coordinates to camera coordinates transformation
            var viewMatrixLocation = GL.GetUniformLocation(_shaderProgram.ProgramId, "viewMatrix");
            var viewMatrix = info.ViewMatrix;
            GL.UniformMatrix4(viewMatrixLocation, false, ref viewMatrix);

            // This is must be here, otherwise the first bound image from BindTextureUnit will display instead of the one that is actually bound
            var samplerArrayLocation = GL.GetUniformLocation(_shaderProgram.ProgramId, "samplers");
            var samplerArray = Enumerable.Range(0, _maxFragmentShaderTextureUnits).ToArray();
            GL.Uniform1(samplerArrayLocation, _maxFragmentShaderTextureUnits, samplerArray);
            
            GL.BindVertexArray(_vertexArrayObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _vertexIndexBuffer);
            GL.DrawElements(PrimitiveType.Triangles, _triangleCount * 3,DrawElementsType.UnsignedInt, 0);
        }

        public void SetState()
        {
            // This dictionary keeps track of existing mapped textures in order to re-use them where appropriate
            var textureObjectIdToShaderIndex = new EqualityComparerMapper<ulong>(EqualityComparer<ulong>.Default);

            _maxFragmentShaderTextureUnits = GL.GetInteger(GetPName.MaxTextureImageUnits);
            _shaderProgram = _shaderCompiler.Compile(vertexShaderCode, GetFragmentShaderCode(_maxFragmentShaderTextureUnits));
            _activeTextures = new List<Textures.Texture>();

            _logger.LogInformation($"Maximum number of fragment shader texture image units = {_maxFragmentShaderTextureUnits}");

            var vertexIndexBufferWriter = new ArrayBufferWriter<uint>();
            var vertexBufferWriter = new ArrayBufferWriter<float>();

            var vertexIndex = 0u;

            foreach (var triangle in _sceneGraph.GetCompleteTriangles())
            {
                var vertices = triangle.GetVertices();

                uint loadVertexPosition(Core.Scene.Vertex v)
                {
                    _sceneGraph.SurfaceVertexTexCoords.TryGetValue((triangle.ObjectId, v.ObjectId), out TexCoords texCoords);
                    _sceneGraph.SurfaceVertexColors.TryGetValue((triangle.ObjectId, v.ObjectId), out ColorRgba vertexColor);

                    ulong? textureSamplerIndex = null;

                    if (texCoords != default)
                    {
                        var textureId = texCoords.TextureId;
                        if (_sceneGraph.Textures.TryGetValue(textureId, out Core.Scene.Texture texture)
                            && !textureObjectIdToShaderIndex.TryGetMappedValue(texture.ObjectId, out textureSamplerIndex))
                        {
                            _activeTextures.Add(_textureLoader.LoadTexture(texture));
                            textureSamplerIndex = textureObjectIdToShaderIndex.Map(texture.ObjectId);
                        }
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
                        vertexColor?.Red ?? 1.0f,
                        vertexColor?.Green ?? 1.0f,
                        vertexColor?.Blue ?? 1.0f,
                        vertexColor?.Opacity ?? 1.0f,
                        // Texture coords
                        texCoords == default ? 0.0f : texCoords.U,
                        texCoords == default ? 0.0f : texCoords.V,
                        // Texture sampler index
                        textureSamplerIndex.HasValue ? textureSamplerIndex.Value - MapConstants.FirstMappableInteger : -1.0f // integer mapper maps from FirstMappableInteger
                    });
                    vertexBufferWriter.Write(vertexComponents);

                    vertexIndex++;
                    return vertexIndex - 1;
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

            if (_activeTextures.Count > _maxFragmentShaderTextureUnits)
            {
                _logger.LogError("Maximum number of active textures exceeded.");
            }

            const int componentsPerVertex = 12; // number of floats per vertex
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

            // Texture coordinates
            const int layout_tex_coords = 4;
            GL.EnableVertexArrayAttrib(buffers[0], layout_tex_coords);
            GL.VertexAttribPointer(layout_tex_coords, 2, VertexAttribPointerType.Float, false, stride, 9 * sizeof(float));

            // Sampler index
            const int layout_sampler_index = 5;
            GL.EnableVertexArrayAttrib(buffers[0], layout_sampler_index);
            GL.VertexAttribPointer(layout_sampler_index, 1, VertexAttribPointerType.Float, false, stride, 11 * sizeof(float));

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

