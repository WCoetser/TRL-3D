using OpenTK.Mathematics;

namespace Trl_3D.Core.Scene.Updates
{
    public record ViewMatrixUpdate(Matrix4 NewViewMatrix) : ISceneGraphUpdate;

    // TODO: Break this down for different assertions
    public record ContentUpdate(SceneGraph SceneGraph) : ISceneGraphUpdate;
}
