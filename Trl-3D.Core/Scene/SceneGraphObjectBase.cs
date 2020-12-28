using Trl_3D.Core.Abstractions;

namespace Trl_3D.Core.Scene
{
    public class SceneGraphObjectBase : ObjectIdentityBase
    {
        protected SceneGraph SceneGraph { get; }

        public SceneGraphObjectBase(SceneGraph sceneGraph, ulong objectId) : base(objectId)
        {
            SceneGraph = sceneGraph;
        }
    }
}
