using System;
using System.Threading;
using Trl_3D.Core.Abstractions;

namespace Trl_3D.Core.Threading
{
    public class CancellationTokenManager : ICancellationTokenManager, IDisposable
    {
        private readonly CancellationTokenSource _cancellationTokenSource;

        public CancellationToken CancellationToken => _cancellationTokenSource.Token;

        public bool IsCancellationRequested => _cancellationTokenSource.IsCancellationRequested;

        public CancellationTokenManager()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        public void Dispose()
        {
            _cancellationTokenSource.Dispose();
        }

        public void CancelToken()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
