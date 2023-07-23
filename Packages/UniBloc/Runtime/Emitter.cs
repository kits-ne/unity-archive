using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityEngine.Assertions;

namespace UniBloc
{
    public interface IEmitter : IDisposable
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
    public class Emitter<TState> : IEmitter<TState>
    {
        public Emitter(Action<TState> emit)
        {
            _emit = emit;
        }

        private readonly Action<TState> _emit;
        private readonly UniTaskCompletionSource _completer = new();
        private readonly List<IDisposable> _disposables = new();

        private bool _isCanceled = false;
        private bool _isCompleted = false;

        public UniTask CompleteTask => _completer.Task;

        public UniTask OnEach<T>(Stream<T> stream, Action<T> onData, Action<Exception> onError = null)
        {
            var completer = new UniTaskCompletionSource();
            IDisposable subscription = stream.Subscribe(
                onData,
                onDone: () => completer.TrySetResult(),
                onError: onError ?? (_ => completer.TrySetException(_)),
                cancelOnError: onError == null
            );

            _disposables.Add(subscription);
            return UniTask.WhenAny(completer.Task, completer.Task)
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
                onData: _ => Emit(onData(_)),
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
            _completer.TrySetResult();
        }
    }
}