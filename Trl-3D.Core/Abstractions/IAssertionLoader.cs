using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Trl_3D.Core.Abstractions
{
    /// <summary>
    /// Implement this interface to specify a source of render assertions.
    /// </summary>
    public interface IAssertionLoader
    {
        /// <summary>
        /// The channel render assertions are written to by the loader.
        /// </summary>
        Channel<IAssertion> AssertionUpdatesChannel { get; }

        /// <summary>
        /// Loads assertions into the <see cref="Channel"/> for async consumption.
        /// </summary>
        /// <returns></returns>
        Task StartAssertionProducer(CancellationToken cancellationToken);
    }
}
