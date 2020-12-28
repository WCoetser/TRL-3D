﻿using System.Collections.Generic;
using System.Linq;

namespace Trl_3D.Core.Scene
{
    public class SceneGraph
    {
        public ColorRgb RgbClearColor { get; set; }
        public Dictionary<ulong, Vertex> Vertices { get; private set; }        
        public Dictionary<ulong, Triangle> Triangles { get; private set; }

        public SceneGraph()
        {
            RgbClearColor = new (1.0f, 1.0f, 1.0f);
            Triangles = new Dictionary<ulong, Triangle>();
            Vertices = new Dictionary<ulong, Vertex>();
        }

        public IEnumerable<Triangle> GetCompleteTriangles()
        {
            return Triangles.Where(pair => IsTriangleReady(pair.Key))
                .Select(pair => pair.Value);
        }

        private bool IsTriangleReady(ulong triangleId)
        {
            return Triangles.TryGetValue(triangleId, out Triangle triangle)
                && Vertices.ContainsKey(triangle.VertexIds.VertexId1)
                && Vertices.ContainsKey(triangle.VertexIds.VertexId2)
                && Vertices.ContainsKey(triangle.VertexIds.VertexId3);
        }
    }
}
