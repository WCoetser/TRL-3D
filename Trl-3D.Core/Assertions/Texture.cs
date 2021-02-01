using Trl_3D.Core.Abstractions;

namespace Trl_3D.Core.Assertions
{
    public class Texture : ObjectIdentityBase, IAssertion
    {
        public Texture(ulong TextureId, string Uri): base(TextureId)
        {
            this.TextureId = TextureId;
            this.Uri = Uri;
        }

        public ulong TextureId { get; }
        public string Uri { get; }
    }
}
