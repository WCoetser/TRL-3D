using System.Threading;
using System.Threading.Tasks;

namespace Trl_3D.Core.Abstractions
{
    public interface IEventProcessor
    {
        Task StartEventProcessor(CancellationToken cancelationToken);
    }
}
