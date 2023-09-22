using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace UniBloc
{
    public static class AsyncEnumerableExtensions
    {
        public static IUniTaskAsyncEnumerable<T> ToUniTaskAsyncEnumerable<T>(this IAsyncEnumerable<T> source)
        {
            return new AsyncToUniTaskAsync<T>(source);
        }

        class AsyncToUniTaskAsync<T> : IUniTaskAsyncEnumerable<T>
        {
            private readonly IAsyncEnumerable<T> _source;

            public AsyncToUniTaskAsync(IAsyncEnumerable<T> source)
            {
                _source = source;
            }

            public IUniTaskAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                return new Enumerator(_source.GetAsyncEnumerator(cancellationToken));
            }

            class Enumerator : IUniTaskAsyncEnumerator<T>
            {
                private readonly IAsyncEnumerator<T> _source;

                public Enumerator(IAsyncEnumerator<T> source)
                {
                    _source = source;
                }

                public UniTask DisposeAsync()
                {
                    return _source.DisposeAsync().AsTask().AsUniTask();
                }

                public UniTask<bool> MoveNextAsync()
                {
                    Current = _source.Current;
                    return _source.MoveNextAsync().AsTask().AsUniTask();
                }

                public T Current { get; private set; }
            }
        }
    }
}