using OpenTK.Mathematics;
using System;
using System.Threading.Tasks;
using Trl_3D.Core.Abstractions;
using Trl_3D.Core.Assertions;

namespace Trl_3D.Core.Scene
{
    public class AssertionProcessor
    {
        private readonly IImageLoader _imageLoader;

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
            }
            else if (assertion is Assertions.Triangle triangle)
            {
                sceneGraph.Triangles[triangle.TriangleId] = new Triangle(sceneGraph, triangle.TriangleId) { VertexIds = triangle.VertexIds };
            }
            else if (assertion is GrabScreenshot)
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
                sceneGraph.SurfaceVertexTexCoords[texCoords.ObjectIdentifier] = texCoords;
            }
            else if (assertion is SurfaceColor surfaceColor)
            {
                sceneGraph.SurfaceVertexColors[surfaceColor.ObjectIdentifier] = surfaceColor.vertexColor;
            }
            else if (assertion is CameraOrientation cameraOrientation)
            {
                Vector3 eyePosition = new Vector3(cameraOrientation.CameraLocation.X, cameraOrientation.CameraLocation.Y, cameraOrientation.CameraLocation.Z);
                Vector3 eyeVector = new Vector3(cameraOrientation.CameraDirection.dX, cameraOrientation.CameraDirection.dY, cameraOrientation.CameraDirection.dZ);
                Vector3 upVector = new Vector3(cameraOrientation.UpDirection.dX, cameraOrientation.UpDirection.dY, cameraOrientation.UpDirection.dZ);
                var target = eyePosition + eyeVector;
                sceneGraph.ViewMatrix = Matrix4.LookAt(eyePosition, target, upVector);
            }
            else
            {
                throw new ArgumentException($"Unknown assertion type: {assertion.GetType()}");
            }
        }
    }
}
