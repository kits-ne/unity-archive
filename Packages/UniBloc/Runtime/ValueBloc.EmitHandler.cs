using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace UniBloc
{
    public abstract partial class ValueBloc<TID, TEvent, TState>
    {
        readonly struct EmitHandler
        {
            private readonly EmitController _controller;
            private readonly Emitter<TState> _emitter;

            private EmitHandler(EmitController controller)
            {
                _controller = controller;
                _emitter = Emitter<TState>.Rent(_controller.OnEmit);
            }

            private void HandleEvent()
            {
                try
                {
                    _controller.HandleEvent(_emitter);
                }
                catch (Exception e)
                {
                    _controller.OnError(e);
                    throw;
                }
                finally
                {
                    _controller.CompleteEvent(_emitter);
                    _controller.Dispose();
                }
            }

            public static void HandleEvent(EmitController controller)
            {
                new EmitHandler(controller).HandleEvent();
            }
        }

        readonly struct EmitAsyncHandler
        {
            private readonly EmitAsyncController _controller;
            private readonly Emitter<TState> _emitter;

            private EmitAsyncHandler(EmitAsyncController controller)
            {
                _controller = controller;
                _emitter = Emitter<TState>.Rent(_controller.OnEmit);
            }

            private async UniTask HandleEventAsync(CancellationToken cancellationToken)
            {
                try
                {
                    await _controller.HandleEventAsync(_emitter, cancellationToken);
                }
                catch (Exception e)
                {
                    _controller.OnError(e);
                    throw;
                }
                finally
                {
                    _controller.CompleteEvent(_emitter);
                    _controller.Dispose();
                }
            }

            public static UniTask HandleEvent(EmitAsyncController controller, CancellationToken cancellationToken)
            {
                return new EmitAsyncHandler(controller).HandleEventAsync(cancellationToken);
            }
        }
    }
}