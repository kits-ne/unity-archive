using System;
using System.Threading;

namespace UniBloc
{
    internal sealed class CancellationTokenDisposable : IDisposable
    {
        readonly CancellationTokenSource _cts = new CancellationTokenSource();

        public CancellationToken Token => _cts.Token;

        public void Dispose()
        {
            if (!_cts.IsCancellationRequested)
            {
                _cts.Cancel();
            }
        }
    }
}