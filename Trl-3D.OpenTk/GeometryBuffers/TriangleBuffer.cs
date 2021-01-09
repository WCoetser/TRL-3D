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
using OpenTK.Mathematics;

namespace Trl_3D.OpenTk.GeometryBuffers
{
    /// <summary>
    /// Builds and manages memory for triangle vertex buffers
    /// </summary>
    public class TriangleBuffer : IDisposable, IGeometryBuffer
    {
        private readonly ILogger _logger;
        private readonly IShaderCompiler _shaderCompiler;
        private readonly ITextureLoader _textureLoader;
        private readonly SceneGraph _sceneGraph;        

        // OpenGL object IDs
        private int _vertexArrayObject;
        private int _vertexBufferObject;
        private int _maxFragmentShaderTextureUnits;
        private int _vertexIndexBufferId;

        // Local state
        private float[] _vertexBuffer;
        private int _triangleCount;
        private uint[] _vertexIndexBuffer;
        
        private List<Core.Scene.Texture> _activeTextures;
        private List<Textures.Texture> _renderTextures;

        private ShaderProgram _shaderProgram;

        /// <summary>
        /// Builds a buffer based on the given triangles for the given scene graph
        /// </summary>
        /// <param name="sceneGraph">The scene graph for the entire world model.</param>
        /// <param name="triangleAssertionsToRender">The triangles for which a buffer needs to be constructed.</param>
        public TriangleBuffer(SceneGraph sceneGraph, 
                              IEnumerable<Core.Scene.Triangle> triangleAssertionsToRender,
                              ILogger logger, 
                              IShaderCompiler shaderCompiler, 
                              ITextureLoader textureLoader)
        {
            _logger = logger;
            _shaderCompiler = shaderCompiler;
            _textureLoader = textureLoader;
            _sceneGraph = sceneGraph;

            BuildBuffer(triangleAssertionsToRender);
        }

        private void BuildBuffer(IEnumerable<Core.Scene.Triangle> triangleAssertionsToRender)
        {
            // This dictionary keeps track of existing mapped textures in order to re-use them where appropriate
            var textureObjectIdToShaderIndex = new EqualityComparerMapper<ulong>(EqualityComparer<ulong>.Default);

            _activeTextures = new List<Core.Scene.Texture>();

            var vertexIndexBufferWriter = new ArrayBufferWriter<uint>();
            var vertexBufferWriter = new ArrayBufferWriter<float>();

            var vertexIndex = 0u;

            foreach (var triangle in triangleAssertionsToRender)
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
                            _activeTextures.Add(texture);
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
                        textureSamplerIndex.HasValue ? (textureSamplerIndex.Value - MapConstants.FirstMappableInteger) : -1.0f
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

            _vertexBuffer = vertexBufferWriter.WrittenMemory.ToArray();
            _vertexIndexBuffer = vertexIndexBufferWriter.WrittenMemory.ToArray();
        }

        public void SetState()        
        {
            _renderTextures = new List<Textures.Texture>();
            foreach (var texture in _activeTextures)
            {
                _renderTextures.Add(_textureLoader.LoadTexture(texture));
            }

            _maxFragmentShaderTextureUnits = GL.GetInteger(GetPName.MaxTextureImageUnits);
            _shaderProgram = _shaderCompiler.Compile(GetVertexShaderCode(), GetFragmentShaderCode());
            _logger.LogInformation($"Maximum number of fragment shader texture image units = {_maxFragmentShaderTextureUnits}");
            if (_activeTextures.Count > _maxFragmentShaderTextureUnits)
            {
                _logger.LogError("Maximum number of active textures exceeded.");
                return;
            }

            const int componentsPerVertex = 12; // number of floats per vertex
            var stride = componentsPerVertex * sizeof(float);

            var vertexArrays = new int[1];
            GL.CreateVertexArrays(1, vertexArrays);
            GL.BindVertexArray(vertexArrays[0]);
            _vertexArrayObject = vertexArrays[0];

            var buffers = new int[1];
            GL.CreateBuffers(1, buffers);
            GL.BindBuffer(BufferTarget.ArrayBuffer, buffers[0]);
            GL.BufferData(BufferTarget.ArrayBuffer, _vertexBuffer.Length * sizeof(float), _vertexBuffer, BufferUsageHint.StaticCopy);
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

            // Vertex index buffer for triangles
            var indexBuffers = new int[1];
            GL.CreateBuffers(1, indexBuffers);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBuffers[0]);
            GL.BufferData(BufferTarget.ElementArrayBuffer, _vertexIndexBuffer.Length * sizeof(uint), _vertexIndexBuffer, BufferUsageHint.StaticDraw);
            _vertexIndexBufferId = indexBuffers[0];

            // Log stats
            _logger.LogInformation($"Vertex buffer size = {_vertexBuffer.Length * sizeof(float)} bytes");
            _logger.LogInformation($"Vertex index buffer size = {_vertexIndexBuffer.Length * sizeof(uint)} bytes");
        }

        public void Render(RenderInfo info)
        {
            Render(info, false);
        }

        public void Render(RenderInfo info, bool isInPickingMode)
        {
            _shaderProgram.UseProgram();

            // Set camera location and projection
            _shaderProgram.SetUniform("viewMatrix", _sceneGraph.ViewMatrix);
            _shaderProgram.SetUniform("projectionMatrix", _sceneGraph.ProjectionMatrix);
            _shaderProgram.SetUniform("isInPickingMode", isInPickingMode);

            // TODO: Remove ToArray()
            GL.BindTextures(0, _renderTextures.Count, _renderTextures.Select(tex => tex.OpenGLTextureId).ToArray());

            // This is must be here, otherwise the first bound image from BindTextureUnit will display instead of the one that is actually bound
            var samplerArray = Enumerable.Range(0, _renderTextures.Count).ToArray();
            _shaderProgram.SetUniform("samplers", samplerArray);

            GL.BindVertexArray(_vertexArrayObject);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, _vertexIndexBufferId);
            GL.DrawElements(PrimitiveType.Triangles, _triangleCount * 3, DrawElementsType.UnsignedInt, 0);
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

        /// <summary>
        /// Must be called in render loop.
        /// </summary>
        public PickingInfo RenderForPicking(RenderInfo info, int screenX, int screenY)
        {
            // Render with picking mode so that surface IDs are generated as colours
            Render(info, true);

            // Get surface ID
            byte[] backBufferDump = new byte[4];
            GL.ReadBuffer(ReadBufferMode.Back);
            int screenYInverted = info.Height - screenY - 1;
            GL.ReadPixels(screenX, screenYInverted, 1, 1, PixelFormat.Rgba, PixelType.UnsignedByte, backBufferDump);

            // TODO: Get depth buffer value to un-project back to world coordinates, requires reading depth buffer
            //float[] zValue = new float[1];
            //GL.ReadPixels(screenX, screenYInverted, 1, 1, PixelFormat.DepthComponent, PixelType.Float, zValue);

            // In this case nothing has been hit and we are looking at the clear colour
            if (backBufferDump[0] == 0
                && backBufferDump[1] == 0
                && backBufferDump[2] == 0
                && backBufferDump[3] == 0)
            {
                return new PickingInfo(null, info.TotalRenderTime, screenX, screenY); ;
            }

            ulong surfaceIdOut = ((ulong)backBufferDump[0] * 256 * 256) + ((ulong)backBufferDump[1] * 256) + (ulong)backBufferDump[2];

            return new PickingInfo(surfaceIdOut, info.TotalRenderTime, screenX, screenY);
        }
    }
}
