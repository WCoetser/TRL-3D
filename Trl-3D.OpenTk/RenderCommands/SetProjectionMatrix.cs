using OpenTK.Mathematics;
using Trl_3D.Core.Abstractions;
using Trl_3D.Core.Scene;

namespace Trl_3D.OpenTk.RenderCommands
{
    internal class SetProjectionMatrix : IRenderCommand
    {
        private readonly float _fieldOfViewVerticalRadians;
        private readonly float _nearPlane;
        private readonly float _farPlane;
        private readonly SceneGraph _sceneGraph;

        public SetProjectionMatrix(SceneGraph sceneGraph, float fieldOfViewVerticalDegrees, float nearPlane, float farPlane)
        {
            _fieldOfViewVerticalRadians = MathHelper.DegreesToRadians(fieldOfViewVerticalDegrees);
            _nearPlane = nearPlane;
            _farPlane = farPlane;
            _sceneGraph = sceneGraph;
        }

        public RenderProcessPosition ProcessStep => RenderProcessPosition.BeforeContent;

        public bool SelfDestruct => false;

        public void Render(RenderInfo renderInfo)
        {
            // This needs to be in the render method because aspect ratio can change after window resize
            _sceneGraph.ProjectionMatrix = Matrix4.CreatePerspectiveFieldOfView(_fieldOfViewVerticalRadians,
                (float)renderInfo.Width / renderInfo.Height, _nearPlane, _farPlane);
        }

        public PickingInfo RenderForPicking(RenderInfo renderInfo, int screenX, int screenY)
        {
            return null;
        }

        public void SetState(RenderInfo renderInfo)
        {
        }
    }
}