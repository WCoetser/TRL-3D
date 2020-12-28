using System;
using Trl_3D.Core.Abstractions;
using Trl_3D.Core.Assertions;

namespace Trl_3D.Core.Scene
{
    public class AssertionProcessor
    {
        /// <summary>
        /// Updates the scenegraph with the information from the assertion.
        /// </summary>
        public void Process(IAssertion assertion, SceneGraph sceneGraph)
        {
            if (assertion is ClearColor clearColor)
            {
                sceneGraph.RgbClearColor = new (clearColor.Red, clearColor.Green, clearColor.Blue);
            }
            else if (assertion is Assertions.Vertex vertex)
            {
                sceneGraph.Vertices[vertex.vertexId] = new Vertex(sceneGraph, vertex.vertexId) { Coordinates = vertex.Coordinates };
            }
            else if (assertion is Assertions.Triangle triangle)
            {
                sceneGraph.Triangles[triangle.triangleId] = new Triangle(sceneGraph, triangle.triangleId) { VertexIds = triangle.VertexIds };
            }
            else if (assertion is GrabScreenshot grabScreenshot)
            {
                // Nothing to do here, screenshots passed out to event processor via render window event channel
            }
            else
            {
                throw new ArgumentException("Unknown assertion type");
            }
        }
    }
}
