﻿using OpenTK.Mathematics;
using System.Collections.Generic;
using Trl_3D.Core.Assertions;

namespace Trl_3D.Core.Scene
{
    /// <summary>
    /// The SceneGraph is a declarative global data structure that caters for partially loaded 
    /// dependencies by using object IDs instead of object references on the heap. Assertions
    /// are aggregated into the scene graph objects in this class.
    /// </summary>
    public class SceneGraph
    {
        public ColorRgba RgbClearColor { get; set; }
        public Dictionary<ulong, Vertex> Vertices { get; }        
        public Dictionary<ulong, Triangle> Triangles { get; }
        public Dictionary<ulong, Texture> Textures { get; }
        public Dictionary<(ulong triangleId, ulong vertexId), TexCoords> SurfaceVertexTexCoords { get; }
        public Dictionary<(ulong triangleId, ulong vertexId), SurfaceColor> SurfaceVertexColors { get; }
        
        // TODO: Copy this to freeze matrix during render step
        public Matrix4 ViewMatrix { get; set; }

        public Matrix4 ProjectionMatrix { get; set; }

        public SceneGraph()
        {
            RgbClearColor = new (0.0f, 0.0f, 0.0f, 1.0f);
            Triangles = new ();
            Vertices = new ();
            Textures = new ();
            SurfaceVertexTexCoords = new ();
            SurfaceVertexColors = new ();
            ViewMatrix = CameraOrientation.Default.ToMatrix();
            ProjectionMatrix = Matrix4.Identity;
        }               
    }
}
