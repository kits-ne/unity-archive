using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace UniBloc
{
    public static class StreamExtensions
    {
        public static Stream<T> ToStream<T>(this IAsyncEnumerable<T> source)
        {
            return new Stream<T>(new AsyncEnumerableSubscriber<T>(source));
        }

        public static Stream<T> ToStream<T>(this IUniTaskAsyncEnumerable<T> source)
        {
            return new Stream<T>(new UniTaskAsyncEnumerableSubscriber<T>(source));
        }
    }
}