using System.Linq;
using Trl_3D.Core.Abstractions;

namespace Trl_3D.Core.Assertions
{
    public class TexCoords : ObjectIdentityBase, IAssertion
    {
        public TexCoords((ulong SurfaceId, ulong VertexId) objectIdentifier, ulong textureId, float u, float v) 
            : base(objectIdentifier.SurfaceId, objectIdentifier.VertexId)
        {
            TextureId = textureId;
            U = u;
            V = v;
        }

        public ulong TextureId { get; }
        public float U { get; }
        public float V { get; }
        public (ulong SurfaceId, ulong VertexId) ObjectIdentifier => (ObjectIds[0], ObjectIds[1]);
    }
}
