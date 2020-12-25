using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Trl_3D.Core.Abstractions
{
    public interface IScene
    {
        /// <summary>
        /// The channel render assertions are written to by the loader.
        /// </summary>
        Channel<AssertionBatch> AssertionUpdatesChannel { get; }

        /// <summary>
        /// Loads scene and starts rendering process.
        /// This is expected to run async, see <see cref="SceneGraph.SceneGraph"/> and <see cref="IAssertionLoader"/>
        /// </summary>
        Task StartAssertionConsumer();
    }
}
