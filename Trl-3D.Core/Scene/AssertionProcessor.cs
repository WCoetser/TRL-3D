using System;
using System.Threading.Tasks;
using Trl_3D.Core.Abstractions;
using Trl_3D.Core.Assertions;

namespace Trl_3D.Core.Scene
{
    public class AssertionProcessor
    {
        private IImageLoader _imageLoader;

        public AssertionProcessor(IImageLoader imageLoader)
        {
            _imageLoader = imageLoader;
        }

        /// <summary>
        /// Updates the scenegraph with the information from the assertion.
        /// </summary>
        public async Task Process(IAssertion assertion, SceneGraph sceneGraph)
        {
            if (assertion is ClearColor clearColor)
            {
                sceneGraph.RgbClearColor = new (clearColor.Red, clearColor.Green, clearColor.Blue, 1.0f);
            }
            else if (assertion is Assertions.Vertex assertionVertex)
            {
                if (!sceneGraph.Vertices.TryGetValue(assertionVertex.VertexId, out Vertex vertex)) {
                    vertex = new Vertex(sceneGraph, assertionVertex.VertexId);
                    sceneGraph.Vertices[assertionVertex.VertexId] = vertex;
                }
                if (assertionVertex.Coordinates != default)
                {
                    vertex.Coordinates = assertionVertex.Coordinates;
                }
                if (assertionVertex.Color != default)
                {
                    vertex.Color = assertionVertex.Color;
                }
            }
            else if (assertion is Assertions.Triangle triangle)
            {
                sceneGraph.Triangles[triangle.TriangleId] = new Triangle(sceneGraph, triangle.TriangleId) { VertexIds = triangle.VertexIds };
            }
            else if (assertion is GrabScreenshot grabScreenshot)
            {
                // Nothing to do here, screenshots passed out to event processor via render window event channel
            }
            else if (assertion is Assertions.Texture texture)
            {
                // Pre-load texture to avoid doing this in the function setting OpenGL state
                var loadedImage = await _imageLoader.LoadImage(new Uri(texture.Uri));
                sceneGraph.Textures[texture.TextureId] = new Texture(sceneGraph, texture.TextureId, loadedImage.BufferRgba, 
                    loadedImage.Width, loadedImage.Height);
            }
            else if (assertion is TexCoords texCoords)
            {
                sceneGraph.TextureCoordinates[(texCoords.SurfaceId, texCoords.VertexId)] = texCoords;
            }
            else
            {
                throw new ArgumentException($"Unknown assertion type: {assertion.GetType()}");
            }
        }
    }
}
