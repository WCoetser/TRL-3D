using System.Collections.Generic;
using Trl_3D.Core.Abstractions;

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
    }
}
