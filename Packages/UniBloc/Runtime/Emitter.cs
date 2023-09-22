using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.Assertions;

namespace UniBloc
{
    public interface IEmitter
    {
        UniTask OnEach<T>(
            Stream<T> stream,
            Action<T> onData,
            Action<Exception> onError = null
        );

        UniTask CompleteTask { get; }
        bool IsDone { get; }
    }

    public interface IEmitter<in TState> : IEmitter
    {
        UniTask ForEach<T>(
            Stream<T> stream,
            Func<T, TState> onData,
            Func<Exception, TState> onError = null
        );

        void Emit(TState state);
    }

    // Completer -> TaskCompletionSource
    public class Emitter<TState> : IEmitter<TState>, IDisposable
    {
        private static readonly Queue<Emitter<TState>> Pool = new(10);

        public static Emitter<TState> Rent(Action<TState> emit)
        {
            Emitter<TState> emitter;
            if (Pool.Any())
            {
                emitter = Pool.Dequeue();
                emitter._emit = emit;
            }
            else
            {
                emitter = new Emitter<TState>(emit);
            }

            return emitter;
        }

        private static void Return(Emitter<TState> emitter)
        {
            emitter._completer = null;
            emitter._isCanceled = false;
            emitter._isCompleted = false;
            Pool.Enqueue(emitter);
        }

        private Emitter(Action<TState> emit)
        {
            _emit = emit;
        }

        private Action<TState> _emit;
        private UniTaskCompletionSource _completer;
        private readonly List<IDisposable> _disposables = new();

        private bool _isCanceled = false;
        private bool _isCompleted = false;

        public UniTask CompleteTask => _completer?.Task ?? UniTask.CompletedTask;

        public UniTask OnEach<T>(Stream<T> stream, Action<T> onData, Action<Exception> onError = null)
        {
            var eachCompleter = new UniTaskCompletionSource();
            var subscription = stream.Subscribe(
                onData,
                onDone: () => eachCompleter.TrySetResult(),
                onError: onError ?? (error => eachCompleter.TrySetException(error)),
                cancelOnError: onError == null
            );

            _disposables.Add(subscription);
            _completer = new UniTaskCompletionSource();
            return UniTask.WhenAny(_completer.Task, eachCompleter.Task)
                .ContinueWith(_ =>
                {
                    subscription.Dispose();
                    _disposables.Remove(subscription);
                });
        }

        public UniTask ForEach<T>(Stream<T> stream, Func<T, TState> onData, Func<Exception, TState> onError = null)
        {
            return OnEach(
                stream,
                onData: data => Emit(onData(data)),
                onError: onError != null
                    ? (error) => Emit(onError(error))
                    : null
            );
        }

        public void Emit(TState state)
        {
            // https://github.com/felangel/bloc/blob/3c97a5749eb2f9d0bafd0ce3df0ca06e8765c7ba/packages/bloc/lib/src/emitter.dart#L113
            try
            {
                Assert.IsFalse(_isCompleted, $"emit was called after an event handler completed normally.");
            }
            catch (Exception)
            {
                Cancel();
                throw;
            }

            if (!_isCanceled)
            {
                _emit(state);
            }
        }

        public bool IsDone => _isCanceled || _isCompleted;

        public void Cancel()
        {
            if (IsDone) return;
            _isCanceled = true;
            (this as IDisposable).Dispose();
        }

        public void Complete()
        {
            if (IsDone) return;
            // https://github.com/felangel/bloc/blob/3c97a5749eb2f9d0bafd0ce3df0ca06e8765c7ba/packages/bloc/lib/src/emitter.dart#L149
            try
            {
                Assert.IsTrue(!_disposables.Any(), "An event handler completed but left pending subscriptions behind.");
            }
            catch (Exception)
            {
                Cancel();
                throw;
            }

            _isCompleted = true;
            (this as IDisposable).Dispose();
        }

        void IDisposable.Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }

            _disposables.Clear();
            _completer?.TrySetResult();
            _emit = null;
            Return(this);
        }
    }
}