using System.Collections.Generic;

namespace Trl_3D.Core.Abstractions
{
    /// <summary>
    /// Process assertions in batches to avoid a scenario where you
    /// have to load millions of vertices through a thread channel.
    /// </summary>
    public class AssertionBatch
    {
        public IEnumerable<IAssertion> Assertions { get; set; }
    }
}
