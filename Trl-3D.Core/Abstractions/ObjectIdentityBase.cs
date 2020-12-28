using System.Diagnostics.CodeAnalysis;
using Trl_3D.Core.Abstractions;

namespace Trl_3D.Core.Abstractions
{
    public abstract class ObjectIdentityBase : IObjectIdentity
    {
        public ulong ObjectId { get; }

        protected ObjectIdentityBase(ulong objectId)
        {
            ObjectId = objectId;
        }

        public override int GetHashCode() => GetHashCode(this);

        public override bool Equals(object obj) => obj is IObjectIdentity idObj && Equals(this, idObj);

        public static bool operator ==(ObjectIdentityBase lhs, ObjectIdentityBase rhs)
        {
            return (lhs, rhs) switch
            {
                (null, null) => true,
                (_, null) => false,
                (null, _) => false,
                _ => lhs.ObjectId == rhs.ObjectId
            };
        }

        public static bool operator !=(ObjectIdentityBase lhs, ObjectIdentityBase rhs) => !(lhs == rhs);

        public bool Equals(IObjectIdentity x, IObjectIdentity y) => x == y;

        public int GetHashCode([DisallowNull] IObjectIdentity obj) => obj.ObjectId.GetHashCode();
    }
}
