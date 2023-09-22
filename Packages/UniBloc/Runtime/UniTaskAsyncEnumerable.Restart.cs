using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace UniBloc
{
    public static class UniTaskAsyncEnumerableExtensions
    {
        public static IUniTaskAsyncEnumerable<(TSource, CancellationToken)> Restart<TSource>(
            this IUniTaskAsyncEnumerable<TSource> source, CancellationToken cancellationToken)
        {
            return new RestartOperator<TSource>(source, cancellationToken);
        }

        sealed class RestartOperator<TSource> : IUniTaskAsyncEnumerable<(TSource, CancellationToken)>
        {
            readonly IUniTaskAsyncEnumerable<TSource> _source;
            private readonly CancellationToken _externalToken;

            public RestartOperator(IUniTaskAsyncEnumerable<TSource> source, CancellationToken externalToken)
            {
                _source = source;
                _externalToken = externalToken;
            }

            public IUniTaskAsyncEnumerator<(TSource, CancellationToken)> GetAsyncEnumerator(
                CancellationToken cancellationToken = default)
            {
                return new Restart(_source, cancellationToken, _externalToken);
            }


            private sealed class Restart : IUniTaskAsyncEnumerator<(TSource, CancellationToken)>
            {
                private readonly IUniTaskAsyncEnumerable<TSource> _source;
                private readonly CancellationToken _enumeratorToken;
                private readonly CancellationToken _externalToken;

                private Channel<(TSource, CancellationToken)> _channel;
                private IUniTaskAsyncEnumerator<(TSource, CancellationToken)> _channelEnumerator;
                private IUniTaskAsyncEnumerator<TSource> _sourceEnumerator;
                private bool _channelClosed;


                public Restart(IUniTaskAsyncEnumerable<TSource> source,
                    CancellationToken enumeratorToken,
                    CancellationToken externalToken)
                {
                    _source = source;
                    _enumeratorToken = enumeratorToken;
                    _externalToken = externalToken;
                }

                public (TSource, CancellationToken) Current => _channelEnumerator.Current;

                private CancellationTokenSource _linkedCts;
                private CancellationTokenSource _recentCts;

                public UniTask<bool> MoveNextAsync()
                {
                    _enumeratorToken.ThrowIfCancellationRequested();

                    if (_sourceEnumerator == null)
                    {
                        _linkedCts = CancellationTokenSource.CreateLinkedTokenSource(_enumeratorToken, _externalToken);
                        _sourceEnumerator = _source.GetAsyncEnumerator(_linkedCts.Token);
                        _channel = Channel.CreateSingleConsumerUnbounded<(TSource, CancellationToken)>();
                        _channelEnumerator = _channel.Reader.ReadAllAsync().GetAsyncEnumerator(_enumeratorToken);
                        ConsumeAll(_sourceEnumerator, _channel).Forget();
                    }

                    return _channelEnumerator.MoveNextAsync();
                }

                async UniTaskVoid ConsumeAll(
                    IUniTaskAsyncEnumerator<TSource> enumerator,
                    ChannelWriter<(TSource, CancellationToken)> writer
                )
                {
                    try
                    {
                        while (await enumerator.MoveNextAsync())
                        {
                            _recentCts?.Cancel();
                            _recentCts = new();
                            var cts = CancellationTokenSource.CreateLinkedTokenSource(_recentCts.Token,
                                _linkedCts.Token);
                            writer.TryWrite((enumerator.Current, cts.Token));
                        }

                        writer.TryComplete();
                    }
                    catch (Exception ex)
                    {
                        writer.TryComplete(ex);
                    }
                    finally
                    {
                        _channelClosed = true;
                        await enumerator.DisposeAsync();
                    }
                }

                public async UniTask DisposeAsync()
                {
                    _linkedCts?.Dispose();

                    if (_sourceEnumerator != null)
                    {
                        await _sourceEnumerator.DisposeAsync();
                    }

                    if (_channelEnumerator != null)
                    {
                        await _channelEnumerator.DisposeAsync();
                    }

                    if (!_channelClosed)
                    {
                        _channelClosed = true;
                        _channel.Writer.TryComplete(new OperationCanceledException());
                    }
                }
            }
        }
    }
}