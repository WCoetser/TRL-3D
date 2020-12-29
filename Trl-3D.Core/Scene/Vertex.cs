using Trl_3D.Core.Assertions;

namespace Trl_3D.Core.Scene
{
    public class Vertex : SceneGraphObjectBase
    {
        public Vertex(SceneGraph sceneGraph, ulong objectId) : base(sceneGraph, objectId)
        {
        }

        public Coordinate3d Coordinates { get; set; }

        public ColorRgba Color { get; set; }
    }
}
