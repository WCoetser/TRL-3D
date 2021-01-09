namespace Trl_3D.Core.Scene
{
    public class Triangle : SceneGraphObjectBase
    {
        public Triangle(SceneGraph sceneGraph, ulong objectId) : base(sceneGraph, objectId)
        {
            // TODO: Increase width of objectID in vertex buffer by splitting it across multiple floats
            const ulong max = (2u << 23) - 1;
            if (objectId > max)
            {
                throw new System.ArgumentException($"ObjectID too large, current limit is {max} imposed by IEEE 754");
            }
        }

        public (ulong VertexId1, ulong VertexId2, ulong VertexId3) VertexIds { get; set; }

        public (Vertex, Vertex, Vertex) GetVertices()
        {
            return (
                SceneGraph.Vertices[VertexIds.VertexId1],
                SceneGraph.Vertices[VertexIds.VertexId2],
                SceneGraph.Vertices[VertexIds.VertexId3]
            );
        }
    }
}
