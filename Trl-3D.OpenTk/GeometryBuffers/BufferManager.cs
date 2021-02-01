using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using Trl_3D.Core.Abstractions;
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

        // This is used to work out of a new buffer should be created for an object, or whether an existing buffer should be updated.
        private Dictionary<ObjectIdentityBase, HashSet<ObjectIdentityBase>> _objectToGeometryBuffers;

        public BufferManager(SceneGraph sceneGraph, ILogger<BufferManager> logger, IShaderCompiler shaderCompiler, ITextureLoader textureLoader)
        {
            _sceneGraph = sceneGraph;
            _logger = logger;
            _shaderCompiler = shaderCompiler;
            _textureLoader = textureLoader;

            _objectToGeometryBuffers = new Dictionary<ObjectIdentityBase, HashSet<ObjectIdentityBase>>();
        }

        internal void AddAssociation(ObjectIdentityBase sceneGraphObject, ObjectIdentityBase geometryBuffer)
        {
            if (geometryBuffer is not IGeometryBuffer)
            {
                throw new ArgumentException($"{nameof(geometryBuffer)} should be {nameof(IGeometryBuffer)}");
            }

            if (!_objectToGeometryBuffers.TryGetValue(sceneGraphObject, out var bufferCollection))
            {
                bufferCollection = new HashSet<ObjectIdentityBase>();
                _objectToGeometryBuffers[sceneGraphObject] = bufferCollection;
            }
            bufferCollection.Add(geometryBuffer);
        }

        internal RenderTriangleBuffer CreateRenderCommands(IEnumerable<Triangle> triangles)
        {
            var newBuffer = new TriangleBuffer(_sceneGraph, this, triangles, _logger, _shaderCompiler, _textureLoader);
            return new RenderTriangleBuffer(newBuffer);
        }        

        internal IEnumerable<ReloadTriangleBuffer> CreateReloadCommands(List<ObjectIdentityBase> knownUpdateObjects)
        {
            // At this point everything is a triangle buffer
            var returnBuffers = new HashSet<TriangleBuffer>();
            var updateObjectRelations = knownUpdateObjects.Where(obj => _objectToGeometryBuffers.ContainsKey(obj)).Select(obj => _objectToGeometryBuffers[obj]);
            foreach (var bufferCollection in updateObjectRelations)
            {
                returnBuffers.UnionWith(bufferCollection.Cast<TriangleBuffer>());
            }
            return returnBuffers.Select(tb => new ReloadTriangleBuffer(tb));
        }

        internal bool HasExistingTriangleBuffer(Triangle value) => _objectToGeometryBuffers.ContainsKey(value);
    }
}
