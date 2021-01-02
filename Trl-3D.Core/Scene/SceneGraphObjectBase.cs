using Trl_3D.Core.Abstractions;

namespace Trl_3D.Core.Scene
{
    public abstract class SceneGraphObjectBase : ObjectIdentityBase
    {
        protected SceneGraph SceneGraph { get; }

        protected SceneGraphObjectBase(SceneGraph sceneGraph, ulong objectId) : base(objectId)
        {
            SceneGraph = sceneGraph;
        }
    }
}
