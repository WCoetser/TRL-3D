using System.Threading;

namespace Trl_3D.Core.Abstractions
{
    /// <summary>
    /// DI wrapper for <see cref="CancellationTokenSource"/>
    /// </summary>
    public interface ICancellationTokenManager
    {
        CancellationToken CancellationToken { get; }

        bool IsCancellationRequested { get; }

        void CancelToken();
    }
}
