using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;

namespace UniBloc
{
    public class ChannelController<T> : IDisposable
    {
        private readonly Channel<T> _channel;
        private readonly IConnectableUniTaskAsyncEnumerable<T> _multicastSource;
        private readonly IDisposable _connection;

        public bool IsDisposed { get; private set; } = false;
        public UniTask Done => _channel.Reader.Completion;

        public ChannelController()
        {
            _channel = Channel.CreateSingleConsumerUnbounded<T>();
            _multicastSource = _channel.Reader.ReadAllAsync().Publish();
            _connection = _multicastSource.Connect();
        }

        public void Publish(T value)
        {
            ThrowIfDisposed();
            _channel.Writer.TryWrite(value);
        }

        private void ThrowIfDisposed()
        {
            if (IsDisposed)
            {
                throw new ObjectDisposedException(nameof(ChannelController<T>));
            }
        }

        public IUniTaskAsyncEnumerable<T> Source() => _multicastSource;

        public void Dispose()
        {
            if (IsDisposed) return;
            _channel.Writer.TryComplete();
            _connection.Dispose();
            IsDisposed = true;
        }
    }

    public interface ISubscriber<T>
    {
        IDisposable Subscribe(Action<T> action);
        void Subscribe(Observer<T> observer, CancellationToken cancellationToken);
    }

    internal class UniTaskAsyncEnumerableSubscriber<T> : ISubscriber<T>
    {
        private readonly IUniTaskAsyncEnumerable<T> _source;

        public UniTaskAsyncEnumerableSubscriber(IUniTaskAsyncEnumerable<T> source)
        {
            _source = source;
        }

        public IDisposable Subscribe(Action<T> action) => _source.Subscribe(action);

        public void Subscribe(Observer<T> observer, CancellationToken cancellationToken)
            => _source.Subscribe(observer, cancellationToken);
    }

    internal class AsyncEnumerableSubscriber<T> : ISubscriber<T>
    {
        private readonly IAsyncEnumerable<T> _source;

        public AsyncEnumerableSubscriber(IAsyncEnumerable<T> source)
        {
            _source = source;
        }

        public IDisposable Subscribe(Action<T> action) => _source.Subscribe(action);

        public void Subscribe(Observer<T> observer, CancellationToken cancellationToken)
            => _source.Subscribe(observer, cancellationToken);
    }


    public class Observer<T> : IObserver<T>
    {
        private readonly Action<T> _onData;
        private readonly Action _onDone;
        private readonly Action<Exception> _onError;
        private readonly CancellationTokenSource _cts;
        private readonly bool _cancelOnError;

        public Observer(
            Action<T> onData,
            Action onDone,
            Action<Exception> onError,
            CancellationTokenSource cts,
            bool cancelOnError = false
        )
        {
            _onData = onData;
            _onDone = onDone;
            _onError = onError;
            _cts = cts;
            _cancelOnError = cancelOnError;
        }

        public void OnCompleted() => _onDone?.Invoke();

        public void OnError(Exception error)
        {
            _onError?.Invoke(error);
            if (_cancelOnError && !_cts.IsCancellationRequested)
            {
                _cts.Cancel();
                _cts.Dispose();
            }
        }

        public void OnNext(T value) => _onData?.Invoke(value);
    }
}