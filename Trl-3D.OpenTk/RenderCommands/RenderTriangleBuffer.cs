
using Microsoft.Extensions.Logging;

using Trl_3D.Core.Abstractions;
using Trl_3D.Core.Scene;
using Trl_3D.OpenTk.Textures;
using Trl_3D.OpenTk.Shaders;
using Trl_3D.OpenTk.GeometryBuffers;
using System.Collections.Generic;

namespace Trl_3D.OpenTk.RenderCommands
{
    public class RenderTriangleBuffer : IRenderCommand
    {
        public RenderProcessPosition ProcessStep => RenderProcessPosition.ContentRenderStep;

        public bool SelfDestruct => false;

        private readonly TriangleBuffer _triangleBuffer;

        public RenderTriangleBuffer(ILogger logger, IShaderCompiler shaderCompiler, 
            ITextureLoader textureLoader, SceneGraph sceneGraph,
            IEnumerable<Triangle> triangles)
        {
            _triangleBuffer = new TriangleBuffer(sceneGraph, triangles, logger, shaderCompiler, textureLoader);
        }

        public void Render(RenderInfo info)
        {
            _triangleBuffer.Render(info);
        }

        public void SetState()
        {
            _triangleBuffer.SetState();
        }

        public void Dispose()
        {
            _triangleBuffer.Dispose();
        }
    }
}

