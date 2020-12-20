using System.Threading;
using System.Threading.Tasks;

namespace Trl_3D.Core.Abstractions
{
    public interface IScene
    {
        /// <summary>
        /// Loads scene and starts rendering process.
        /// This is expected to run async, see <see cref="SceneGraph.SceneGraph"/> and <see cref="IAssertionLoader"/>
        /// </summary>
        Task StartAssertionConsumer(CancellationToken cancellationToken);
    }
}
