using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace UniBloc
{
    public readonly struct Stream<T>
    {
        private readonly ISubscriber<T> _subscriber;

        public Stream(ISubscriber<T> subscriber)
        {
            _subscriber = subscriber;
        }

        public IDisposable Subscribe(Action<T> action) => _subscriber.Subscribe(action);

        // public IUniTaskAsyncEnumerable<T> AsAsyncEnumerable() => _source;

        public IDisposable Subscribe(
            Action<T> onData,
            Action onDone,
            Action<Exception> onError,
            bool cancelOnError = false)
        {
            var cts = new CancellationTokenSource();
            var observer = new Observer<T>(
                onData,
                onDone,
                onError,
                cts,
                cancelOnError
            );
            _subscriber.Subscribe(observer, cancellationToken: cts.Token);
            return cts;
        }
    }
}