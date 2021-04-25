using OpenTK.Graphics.OpenGL4;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Trl.IntegerMapper.EqualityComparerIntegerMapper;
using Trl_3D.Core.Abstractions;
using Trl_3D.Core.Scene;
using Trl_3D.OpenTk.Shaders;
using Trl_3D.OpenTk.Textures;
using System.Buffers;
using Trl_3D.Core.Assertions;
using Trl.IntegerMapper;
using System.Linq;
using System.IO;

namespace Trl_3D.OpenTk.GeometryBuffers
{
    /// <summary>
    /// Builds and manages memory for triangle vertex buffers
    /// </summary>
    public class TriangleBuffer : InstanceCounterBase, IDisposable, IGeometryBuffer
    {
        private readonly ILogger _logger;
        private readonly IShaderCompiler _shaderCompiler;
        private readonly ITextureLoader _textureLoader;
        private readonly SceneGraph _sceneGraph;
        private readonly BufferManager _bufferManager;

        // OpenGL object IDs
        private int _vertexArrayObject;
        private int _vertexBufferObject;
        private int _maxFragmentShaderTextureUnits;
        private int _vertexIndexBufferId;

        // Local state
        private Lazy<float[]> _vertexBuffer;
        private Lazy<uint[]> _vertexIndexBuffer;
        private EqualityComparerMapper<ulong> _textureObjectIdToShaderIndex;
        private List<Core.Scene.Texture> _activeTextures;
        private List<Textures.Texture> _renderTextures;

        private ShaderProgram _shaderProgram;

        const int ComponentsPerVertex = 12; // number of floats per vertex
        const int VerticesPerTriangle = 3;

        private readonly Core.Scene.Triangle[] _triangleAssertionsToRender;

        /// <summary>
        /// Builds a buffer based on the given triangles for the given scene graph
        /// </summary>
        /// <param name="sceneGraph">The scene graph for the entire world model.</param>
        /// <param name="triangleAssertionsToRender">The triangles for which a buffer needs to be constructed.</param>
        public TriangleBuffer(SceneGraph sceneGraph, 
                              BufferManager bufferManager,
                              IEnumerable<Core.Scene.Triangle> triangleAssertionsToRender,
                              ILogger logger, 
                              IShaderCompiler shaderCompiler, 
                              ITextureLoader textureLoader) : base()
        {
            _logger = logger;
            _shaderCompiler = shaderCompiler;
            _textureLoader = textureLoader;
            _sceneGraph = sceneGraph;
            _bufferManager = bufferManager;

            _triangleAssertionsToRender = triangleAssertionsToRender.ToArray();

            _vertexBuffer = new Lazy<float[]>(() => new float[_triangleAssertionsToRender.Length * VerticesPerTriangle * ComponentsPerVertex]);
            _vertexIndexBuffer = new Lazy<uint[]>(() => new uint[_triangleAssertionsToRender.Length * VerticesPerTriangle]);

            _textureObjectIdToShaderIndex = new EqualityComparerMapper<ulong>(EqualityComparer<ulong>.Default);

            BuildBuffer();
        }

        private void BuildBuffer()
        {
            _activeTextures = new List<Core.Scene.Texture>();

            int vertexIndex = 0;
            int triangleIndex = 0;

            _textureObjectIdToShaderIndex.Clear();

            foreach (var triangle in _triangleAssertionsToRender)
            {
                _bufferManager.AddAssociation(triangle, this);

                var vertices = triangle.GetVertices();

                int loadVertexPosition(Core.Scene.Vertex v)
                {
                    _bufferManager.AddAssociation(v, this);

                    _sceneGraph.SurfaceVertexTexCoords.TryGetValue((triangle.TriangleId, v.VertexId), out TexCoords texCoords);
                    _sceneGraph.SurfaceVertexColors.TryGetValue((triangle.TriangleId, v.VertexId), out SurfaceColor vertexColor);

                    ulong? textureSamplerIndex = null;

                    if (texCoords != default)
                    {
                        _bufferManager.AddAssociation(texCoords, this);

                        var textureId = texCoords.TextureId;
                        if (_sceneGraph.Textures.TryGetValue(textureId, out Core.Scene.Texture texture)
                            && !_textureObjectIdToShaderIndex.TryGetMappedValue(texture.TextureId, out textureSamplerIndex))
                        {
                            _bufferManager.AddAssociation(texture, this);

                            _activeTextures.Add(texture);
                            textureSamplerIndex = _textureObjectIdToShaderIndex.Map(texture.TextureId);
                        }
                    }

                    if (vertexColor != default)
                    {
                        _bufferManager.AddAssociation(vertexColor, this);
                    }

                    var spanOutVertexData = new Span<float>(_vertexBuffer.Value, vertexIndex * ComponentsPerVertex, ComponentsPerVertex);
                    // Object Ids
                    spanOutVertexData[0] = v.VertexId;
                    spanOutVertexData[1] = triangle.TriangleId;
                    // Coordinates
                    spanOutVertexData[2] = v.Coordinates.X;
                    spanOutVertexData[3] = v.Coordinates.Y;
                    spanOutVertexData[4] = v.Coordinates.Z;
                    // Colour
                    spanOutVertexData[5] = vertexColor?.VertexColor.Red ?? 1.0f;
                    spanOutVertexData[6] = vertexColor?.VertexColor.Green ?? 1.0f;
                    spanOutVertexData[7] = vertexColor?.VertexColor.Blue ?? 1.0f;
                    spanOutVertexData[8] = vertexColor?.VertexColor.Opacity ?? 1.0f;
                    // Texture coords
                    spanOutVertexData[9] = texCoords == default ? 0.0f : texCoords.U;
                    spanOutVertexData[10] = texCoords == default ? 0.0f : texCoords.V;
                    // Texture sampler index
                    spanOutVertexData[11] = textureSamplerIndex.HasValue ? (textureSamplerIndex.Value - MapConstants.FirstMappableInteger) : -1.0f;

                    vertexIndex++;
                    return vertexIndex - 1;
                };

                var spanOutIndexData = new Span<uint>(_vertexIndexBuffer.Value, triangleIndex * VerticesPerTriangle, VerticesPerTriangle);
                spanOutIndexData[0] = (uint)loadVertexPosition(vertices.Item1);
                spanOutIndexData[1] = (uint)loadVertexPosition(vertices.Item2);
                spanOutIndexData[2] = (uint)loadVertexPosition(vertices.Item3);

                triangleIndex++;
            }
        }

        public void SetState(bool isReload)
        {
            if (!isReload)
            {
                _maxFragmentShaderTextureUnits = GL.GetInteger(GetPName.MaxTextureImageUnits);
                _shaderProgram = _shaderCompiler.Compile(GetVertexShaderCode(), GetFragmentShaderCode());
            }

            if (_activeTextures.Count > _maxFragmentShaderTextureUnits)
            {
                _logger.LogInformation($"Maximum number of fragment shader texture image units = {_maxFragmentShaderTextureUnits}");
                _logger.LogError("Maximum number of active textures exceeded.");
                return;
            }

            _renderTextures = new List<Textures.Texture>();
            foreach (var texture in _activeTextures)
            {
                _renderTextures.Add(_textureLoader.LoadTexture(texture));
            }

            if (isReload)
            {
                GL.BindBuffer(BufferTarget.ArrayBuffer, _vertexBufferObject);
                GL.BufferSubData(BufferTarget.ArrayBuffer, IntPtr.Zero, _vertexBuffer.Value.Length * sizeof(float), _vertexBuffer.Value);
            }
            else
            {                
                var stride = ComponentsPerVertex * sizeof(float);

                var vertexArrays = new int[1];
                GL.CreateVertexArrays(1, vertexArrays);
                GL.BindVertexArray(vertexArrays[0]);
                _vertexArrayObject = vertexArrays[0];

                var buffers = new int[1];
                GL.CreateBuffers(1, buffers);
                GL.BindBuffer(BufferTarget.ArrayBuffer, buffers[0]);
                GL.BufferData(BufferTarget.ArrayBuffer, _vertexBuffer.Value.Length * sizeof(float), _vertexBuffer.Value, BufferUsageHint.DynamicCopy);
                _vertexBufferObject = buffers[0];

                // Vertex ID
                const int layout_pos_vertexId = 0;
                GL.EnableVertexArrayAttrib(_vertexArrayObject, layout_pos_vertexId);
                GL.VertexAttribPointer(layout_pos_vertexId, 3, VertexAttribPointerType.Float, false, stride, 0);

                // Surface ID
                const int layout_pos_surfaceId = 1;
                GL.EnableVertexArrayAttrib(_vertexArrayObject, layout_pos_surfaceId);
                GL.VertexAttribPointer(layout_pos_surfaceId, 3, VertexAttribPointerType.Float, false, stride, sizeof(float));

                // Vertex position
                const int layout_pos_vertexPosition = 2;
                GL.EnableVertexArrayAttrib(_vertexArrayObject, layout_pos_vertexPosition);
                GL.VertexAttribPointer(layout_pos_vertexPosition, 3, VertexAttribPointerType.Float, false, stride, 2 * sizeof(float));

                // Vertex colour
                const int layout_pos_vertexColor = 3;
                GL.EnableVertexArrayAttrib(_vertexArrayObject, layout_pos_vertexColor);
                GL.VertexAttribPointer(layout_pos_vertexColor, 4, VertexAttribPointerType.Float, false, stride, 5 * sizeof(float));

                // Texture coordinates
                const int layout_tex_coords = 4;
                GL.EnableVertexArrayAttrib(_vertexArrayObject, layout_tex_coords);
                GL.VertexAttribPointer(layout_tex_coords, 2, VertexAttribPointerType.Float, false, stride, 9 * sizeof(float));

                // Sampler index
                const int layout_sampler_index = 5;
                GL.EnableVertexArrayAttrib(_vertexArrayObject, layout_sampler_index);
                GL.VertexAttribPointer(layout_sampler_index, 1, VertexAttribPointerType.Float, false, stride, 11 * sizeof(float));

                // Create vertex index buffer for drawing triangles
                var indexBuffers = new int[1];
                GL.CreateBuffers(1, indexBuffers);
                GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffers[0]);
                GL.BufferData(BufferTarget.ElementArrayBuffer, _vertexIndexBuffer.Value.Length * sizeof(uint), _vertexIndexBuffer.Value, BufferUsageHint.DynamicDraw);
                _vertexIndexBufferId = indexBuffers[0];

                // Log stats
                _logger.LogInformation($"Vertex buffer size = {_vertexBuffer.Value.Length * sizeof(float)} bytes");
                _logger.LogInformation($"Vertex index buffer size = {_vertexIndexBuffer.Value.Length * sizeof(uint)} bytes");
            }
        }

        public void Render(RenderInfo info)
        {
            _shaderProgram.UseProgram();

            // Set camera location and projection
            _shaderProgram.SetUniform("viewMatrix", info.CurrentViewMatrix);
            _shaderProgram.SetUniform("projectionMatrix", info.CurrentProjectionMatrix);

            // TODO: Remove ToArray()
            GL.BindTextures(0, _renderTextures.Count, _renderTextures.Select(tex => tex.OpenGLTextureId).ToArray());

            // This is must be here, otherwise the first bound image from BindTextureUnit will display instead of the one that is actually bound
            var samplerArray = Enumerable.Range(0, _renderTextures.Count).ToArray();
            _shaderProgram.SetUniform("samplers", samplerArray);

            GL.BindVertexArray(_vertexArrayObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _vertexIndexBufferId);
            GL.DrawElements(PrimitiveType.Triangles, _triangleAssertionsToRender.Length * VerticesPerTriangle, DrawElementsType.UnsignedInt, 0);
        }

        public void Dispose()
        {
            _shaderProgram?.Dispose();

            GL.DeleteVertexArrays(1, ref _vertexArrayObject);
            GL.DeleteBuffer(_vertexBufferObject);
            GL.DeleteBuffer(_vertexIndexBufferId);
        }

        private string GetFragmentShaderCode()
        {
            using var inputStream = GetType().Assembly.GetManifestResourceStream("Trl_3D.OpenTk.GeometryBuffers.triangle_buffer_shader.frag");
            using var reader = new StreamReader(inputStream);
            return reader.ReadToEnd().Replace("{{maxFragmentShaderTextureUnits}}", _maxFragmentShaderTextureUnits.ToString());
        }

        private string GetVertexShaderCode()
        {
            using var inputStream = GetType().Assembly.GetManifestResourceStream("Trl_3D.OpenTk.GeometryBuffers.triangle_buffer_shader.vert");
            using var reader = new StreamReader(inputStream);
            return reader.ReadToEnd();            
        }

        public void Reload()
        {
            BuildBuffer();
            SetState(true);
        }
    }
}
