
using Microsoft.Extensions.Logging;
using System.Linq;

using Trl_3D.Core.Abstractions;
using Trl_3D.Core.Scene;
using Trl_3D.OpenTk.Textures;
using Trl_3D.OpenTk.Shaders;
using Trl_3D.OpenTk.GeometryBuffers;

namespace Trl_3D.OpenTk.RenderCommands
{
    public class RenderSceneGraph : IRenderCommand
    {
        public RenderProcessPosition ProcessStep => RenderProcessPosition.ContentRenderStep;

        public bool SelfDestruct => false;

        private readonly TriangleBuffer _triangleBuffer;

        public RenderSceneGraph(ILogger logger, IShaderCompiler shaderCompiler, ITextureLoader textureLoader, SceneGraph sceneGraph)
        {
            var renderTriangles = sceneGraph.GetCompleteTriangles();

            if (renderTriangles.Count() != sceneGraph.Triangles.Count)
            {
                logger.LogWarning($"Some triangles have missing vertices and will not be rendered.");
            }

            _triangleBuffer = new TriangleBuffer(sceneGraph, renderTriangles, logger, shaderCompiler, textureLoader);
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

