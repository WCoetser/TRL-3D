using System;
using System.Collections.Generic;

namespace Trl_3D.Core.Scene
{
    public class SceneGraph
    {
        public float[] RgbClearColor { get; set; }
        public Dictionary<long, Vertex> Vertices { get; private set; }
        public List<Surface> Surfaces { get; private set; }

        public SceneGraph()
        {
            // Default values for scene objects
            RgbClearColor = new[] { 1.0f, 1.0f, 1.0f };
            Vertices = new Dictionary<long, Vertex>();
            Surfaces = new List<Surface>();
        }
    }
}
