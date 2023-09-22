using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace UniBloc
{
    internal static class Subscribes
    {
        private static readonly Action<Exception> NopError = _ => { };
        private static readonly Action NopCompleted = () => { };

        public static IDisposable Subscribe<T>(this IAsyncEnumerable<T> source, Action<T> action)
        {
            var cts = new CancellationTokenDisposable();
            SubscribeCore(source, action, NopError, NopCompleted, cts.Token).Forget();
            return cts;
        }

        public static void Subscribe<T>(this IAsyncEnumerable<T> source, Observer<T> observer,
            CancellationToken cancellationToken)
        {
            SubscribeCore(source, observer, cancellationToken).Forget();
        }

        private static async UniTaskVoid SubscribeCore<TSource>(IAsyncEnumerable<TSource> source,
            Action<TSource> onNext, Action<Exception> onError, Action onCompleted, CancellationToken cancellationToken)
        {
            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                while (await e.MoveNextAsync())
                {
                    onNext(e.Current);
                }

                onCompleted();
            }
            catch (Exception ex)
            {
                if (onError == NopError || ex is OperationCanceledException)
                {
                    return;
                }

                onError(ex);
            }
            finally
            {
                if (e != null)
                {
                    await e.DisposeAsync();
                }
            }
        }

        private static async UniTaskVoid SubscribeCore<TSource>(IAsyncEnumerable<TSource> source,
            IObserver<TSource> observer, CancellationToken cancellationToken)
        {
            var e = source.GetAsyncEnumerator(cancellationToken);
            try
            {
                while (await e.MoveNextAsync())
                {
                    observer.OnNext(e.Current);
                }

                observer.OnCompleted();
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException) return;
                observer.OnError(ex);
            }
            finally
            {
                if (e != null)
                {
                    await e.DisposeAsync();
                }
            }
        }
    }
}