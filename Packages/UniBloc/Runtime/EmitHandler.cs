using System;
using Cysharp.Threading.Tasks;

namespace UniBloc
{
    public abstract partial class Bloc<TEvent, TState>
    {
        readonly struct EmitHandler<T> where T : TEvent
        {
            private readonly EmitController<T> _controller;
            private readonly Emitter<TState> _emitter;

            private EmitHandler(EmitController<T> controller)
            {
                _controller = controller;
                _emitter = new Emitter<TState>(_controller.OnEmit);
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

            public static void HandleEvent(EmitController<T> controller)
            {
                new EmitHandler<T>(controller).HandleEvent();
            }
        }

        readonly struct EmitAsyncHandler<T> where T : TEvent
        {
            private readonly EmitAsyncController<T> _controller;
            private readonly Emitter<TState> _emitter;

            private EmitAsyncHandler(EmitAsyncController<T> controller)
            {
                _controller = controller;
                _emitter = new Emitter<TState>(_controller.OnEmit);
            }

            private async UniTaskVoid HandleEventAsync()
            {
                try
                {
                    await _controller.HandleEventAsync(_emitter);
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

            public static void HandleEvent(EmitAsyncController<T> controller)
            {
                new EmitAsyncHandler<T>(controller).HandleEventAsync().Forget();
            }
        }
    }
}