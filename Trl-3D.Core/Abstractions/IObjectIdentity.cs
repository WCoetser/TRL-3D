using System.Collections.Generic;

namespace Trl_3D.Core.Abstractions
{
    /// <summary>
    /// Object IDs are used to cater for equality and hashing where 
    /// objects are partially complete.
    /// </summary>
    public interface IObjectIdentity : IEqualityComparer<IObjectIdentity>
    {
        public ulong ObjectId { get; }
    }
}
