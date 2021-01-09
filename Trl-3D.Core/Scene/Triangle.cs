namespace Trl_3D.Core.Scene
{
    public class Triangle : SceneGraphObjectBase
    {
        public Triangle(SceneGraph sceneGraph, ulong objectId) : base(sceneGraph, objectId)
        {
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

        /// <summary>
        /// Returns true if triangle is ready to be sent to rendering system.
        /// </summary>
        public bool HasMinimumRenderInfo
        {
            get
            {
                return SceneGraph.Vertices.ContainsKey(VertexIds.VertexId1)
                    && SceneGraph.Vertices.ContainsKey(VertexIds.VertexId2)
                    && SceneGraph.Vertices.ContainsKey(VertexIds.VertexId3);
            }
        }
    }
}
