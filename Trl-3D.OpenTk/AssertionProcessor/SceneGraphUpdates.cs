using OpenTK.Mathematics;
using Trl_3D.Core.Scene;

namespace Trl_3D.OpenTk.AssertionProcessor
{
    public record ViewMatrixUpdate(Matrix4 NewViewMatrix) : Core.Scene.ISceneGraphUpdate;

    // TODO: Break this down for different assertions
    public record ContentUpdate(SceneGraph SceneGraph) : Core.Scene.ISceneGraphUpdate;
}
