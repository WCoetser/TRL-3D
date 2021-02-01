using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Trl_3D.Core.Abstractions
{
    public abstract class ObjectIdentityBase : IEqualityComparer<ObjectIdentityBase>
    {
        /// <summary>
        /// Compound primary key.
        /// </summary>
        protected IReadOnlyList<ulong> ObjectIds { get; }

        protected ObjectIdentityBase(params ulong[] objectIds)
        {
            ObjectIds = objectIds;
            // TODO: Increase width of objectID in vertex buffer by splitting it across multiple floats
            const ulong max = (2u << 23) - 1;
            if (ObjectIds.Any(objectId => objectId > max))
            {
                throw new ArgumentException($"ObjectID too large, current limit is {max} imposed by IEEE 754");
            }
        }

        public override int GetHashCode() => GetHashCode(this);

        public override bool Equals(object obj) => obj is ObjectIdentityBase idObj && Equals(this, idObj);

        public static bool operator ==(ObjectIdentityBase lhs, ObjectIdentityBase rhs)
        {
            return (lhs, rhs) switch
            {
                (null, null) => true,
                (_, null) => false,
                (null, _) => false,
                _ => (lhs.ObjectIds, lhs.GetType()) == (rhs.ObjectIds, rhs.GetType()) && Enumerable.SequenceEqual(lhs.ObjectIds, rhs.ObjectIds)
            };
        }

        public static bool operator !=(ObjectIdentityBase lhs, ObjectIdentityBase rhs) => !(lhs == rhs);

        public bool Equals(ObjectIdentityBase x, ObjectIdentityBase y) => x == y;

        public int GetHashCode([DisallowNull] ObjectIdentityBase obj) => HashCode.Combine(obj.ObjectIds.GetHashCode(), obj.GetType());
    }
}
