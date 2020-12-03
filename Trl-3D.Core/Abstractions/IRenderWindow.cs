using Microsoft.Extensions.Logging;

namespace Trl_3D.Core.Abstractions
{
    public interface IRenderWindow
    {
        void Run();
        void SetLogger(ILogger logger);
    }
}
