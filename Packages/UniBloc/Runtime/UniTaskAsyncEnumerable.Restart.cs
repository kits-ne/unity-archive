using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace UniBloc
{
    public static class UniTaskAsyncEnumerableExtensions
    {
        public static IUniTaskAsyncEnumerable<(TSource, CancellationToken)> Restart<TSource>(
            this IUniTaskAsyncEnumerable<TSource> source)
        {
            return new RestartOperator<TSource>(source);
        }

        sealed class RestartOperator<TSource> : IUniTaskAsyncEnumerable<(TSource, CancellationToken)>
        {
            readonly IUniTaskAsyncEnumerable<TSource> source;

            public RestartOperator(IUniTaskAsyncEnumerable<TSource> source)
            {
                this.source = source;
            }

            public IUniTaskAsyncEnumerator<(TSource, CancellationToken)> GetAsyncEnumerator(
                CancellationToken cancellationToken = default)
            {
                return new _Restart(source, cancellationToken);
            }


            sealed class _Restart : IUniTaskAsyncEnumerator<(TSource, CancellationToken)>
            {
                readonly IUniTaskAsyncEnumerable<TSource> source;
                CancellationToken cancellationToken;

                Channel<(TSource, CancellationToken)> channel;
                IUniTaskAsyncEnumerator<(TSource, CancellationToken)> channelEnumerator;
                IUniTaskAsyncEnumerator<TSource> sourceEnumerator;
                bool channelClosed;

                public _Restart(IUniTaskAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
                {
                    this.source = source;
                    this.cancellationToken = cancellationToken;
                }

                public (TSource, CancellationToken) Current => channelEnumerator.Current;

                private CancellationTokenSource _cts = new();

                public UniTask<bool> MoveNextAsync()
                {
                    cancellationToken.ThrowIfCancellationRequested();


                    if (sourceEnumerator == null)
                    {
                        sourceEnumerator = source.GetAsyncEnumerator(cancellationToken);
                        channel = Channel.CreateSingleConsumerUnbounded<(TSource, CancellationToken)>();

                        channelEnumerator = channel.Reader.ReadAllAsync().GetAsyncEnumerator(cancellationToken);

                        ConsumeAll(this, sourceEnumerator, channel).Forget();
                    }

                    return channelEnumerator.MoveNextAsync();
                }

                async UniTaskVoid ConsumeAll(_Restart self, IUniTaskAsyncEnumerator<TSource> enumerator,
                    ChannelWriter<(TSource, CancellationToken)> writer)
                {
                    try
                    {
                        while (await enumerator.MoveNextAsync())
                        {
                            _cts?.Cancel();
                            _cts = new();
                            writer.TryWrite((enumerator.Current, _cts.Token));
                        }

                        writer.TryComplete();
                    }
                    catch (Exception ex)
                    {
                        writer.TryComplete(ex);
                    }
                    finally
                    {
                        self.channelClosed = true;
                        await enumerator.DisposeAsync();
                    }
                }

                public async UniTask DisposeAsync()
                {
                    if (sourceEnumerator != null)
                    {
                        await sourceEnumerator.DisposeAsync();
                    }

                    if (channelEnumerator != null)
                    {
                        await channelEnumerator.DisposeAsync();
                    }

                    if (!channelClosed)
                    {
                        channelClosed = true;
                        channel.Writer.TryComplete(new OperationCanceledException());
                    }
                }
            }
        }
    }
}