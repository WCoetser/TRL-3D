
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

        public void Render(RenderInfo renderInfo)
        {
            _triangleBuffer.Render(renderInfo);
        }

        public void SetState(RenderInfo renderInfo)
        {
            _triangleBuffer.SetState();
        }

        public void Dispose()
        {
            _triangleBuffer.Dispose();
        }

        public PickingInfo RenderForPicking(RenderInfo renderInfo, int screenX, int screenY)
        {
            return _triangleBuffer.RenderForPicking(renderInfo, screenX, screenY);
        }
    }
}

