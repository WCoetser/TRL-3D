using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Trl_3D.Core.Scene;
using Trl_3D.OpenTk.RenderCommands;
using Trl_3D.OpenTk.Shaders;
using Trl_3D.OpenTk.Textures;

namespace Trl_3D.OpenTk.GeometryBuffers
{
    public class BufferManager
    {
        private readonly SceneGraph _sceneGraph;
        private readonly ILogger<BufferManager> _logger;
        private readonly IShaderCompiler _shaderCompiler;
        private readonly ITextureLoader _textureLoader;

        // TODO ... Dispose & reference count
        private readonly List<TriangleBuffer> _triangleBuffers;

        public BufferManager(SceneGraph sceneGraph, ILogger<BufferManager> logger, IShaderCompiler shaderCompiler, ITextureLoader textureLoader)
        {
            _sceneGraph = sceneGraph;
            _logger = logger;
            _shaderCompiler = shaderCompiler;
            _textureLoader = textureLoader;

            _triangleBuffers = new List<TriangleBuffer>();
        }

        internal RenderTriangleBuffer CreateRenderCommands(IEnumerable<Triangle> triangles)
        {
            var newBuffer = new TriangleBuffer(_sceneGraph, triangles, _logger, _shaderCompiler, _textureLoader);
            _triangleBuffers.Add(newBuffer);
            return new RenderTriangleBuffer(newBuffer);
        }        

        internal IEnumerable<ReloadTriangleBuffer> CreateReloadCommands()
        {
            return _triangleBuffers.Select(tb => new ReloadTriangleBuffer(tb));
        }
    }
}
