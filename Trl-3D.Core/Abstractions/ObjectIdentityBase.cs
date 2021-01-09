using System.Diagnostics.CodeAnalysis;

namespace Trl_3D.Core.Abstractions
{
    public abstract class ObjectIdentityBase : IObjectIdentity
    {
        public ulong ObjectId { get; }

        protected ObjectIdentityBase(ulong objectId)
        {
            ObjectId = objectId;
            // TODO: Increase width of objectID in vertex buffer by splitting it across multiple floats
            const ulong max = (2u << 23) - 1;
            if (objectId > max)
            {
                throw new System.ArgumentException($"ObjectID too large, current limit is {max} imposed by IEEE 754");
            }
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
