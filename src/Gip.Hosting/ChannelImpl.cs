using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;

using Gip.Abstractions;

namespace Gip.Hosting
{

    /// <summary>
    /// Holds a reference to a registered chanel in the channel container.
    /// </summary>
    class ChannelImpl : ILocalChannelHandle
    {

        /// <summary>
        /// Channel reader implementation for a local signal store.
        /// </summary>
        /// <typeparam name="TSignal"></typeparam>
        class ChannelReader<TSignal> : IChannelReader<TSignal>
        {

            readonly ChannelImpl _impl;
            readonly CancellationToken _cancellationToken;

            /// <summary>
            /// Initializes a new instance.
            /// </summary>
            /// <param name="impl"></param>
            public ChannelReader(ChannelImpl impl, CancellationToken cancellationToken)
            {
                _impl = impl;
                _cancellationToken = cancellationToken;
            }

            /// <inheritdoc />
            public IReadableChannelHandle Channel => _impl;

            /// <inheritdoc />
            public async IAsyncEnumerator<TSignal> GetAsyncEnumerator(CancellationToken cancellationToken = default)
            {
                var cts = CancellationTokenSource.CreateLinkedTokenSource(_cancellationToken, cancellationToken);

                await foreach (var signal in ((IChannelStore<TSignal>)_impl.Store).OpenAsync(cts.Token))
                {
                    if (cts.IsCancellationRequested)
                        yield break;
                    else
                        yield return signal;
                }
            }

            /// <inheritdoc />
            public void Dispose()
            {

            }

        }

        readonly ChannelSchema _schema;
        readonly IChannelStore _store;
        readonly Guid _id;

        IChannelWriter? _writer = null;

        IReadableChannelHandle? _bound = null;
        CancellationTokenSource _boundCancellationTokenSource = new CancellationTokenSource();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="store"></param>
        /// <param name="id"></param>
        internal ChannelImpl(ChannelSchema schema, IChannelStore store, Guid id)
        {
            _schema = schema;
            _store = store;
            _id = id;
        }

        /// <summary>
        /// Gets the registered <see cref="IChannelStore"/>.
        /// </summary>
        public IChannelStore Store => _store;

        /// <inheritdoc />
        public Guid Id => _id;

        /// <inheritdoc />
        public ChannelSchema Schema => _schema;

        /// <inheritdoc />
        public async IAsyncEnumerable<IChannelReader<TSignal>> Reader<TSignal>([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            if (typeof(TSignal) != Schema.Signal.Type)
                throw new ArgumentException($"Type {typeof(TSignal)} is not compatible with channel schema type {Schema.Signal}.");

            while (cancellationToken.IsCancellationRequested == false)
            {
                CancellationToken readerCancellationToken = default;
                IChannelReader<TSignal>? reader = null;
                IReadableChannelHandle? bound = null;
                IAsyncEnumerator<IChannelReader<TSignal>>? boundIter = null;

                lock (this)
                {
                    // we always need to be able to cancel the outstanding reader to move to the next
                    readerCancellationToken = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _boundCancellationTokenSource.Token).Token;

                    if (_bound == null)
                    {
                        reader = new ChannelReader<TSignal>(this, readerCancellationToken);
                        bound = null;
                    }
                    else
                    {
                        reader = null;
                        if (bound != _bound)
                        {
                            bound = _bound;
                            boundIter = bound.Reader<TSignal>(readerCancellationToken).GetAsyncEnumerator(readerCancellationToken);
                        }
                    }
                }

                if (reader is not null)
                    yield return reader;
                else if (boundIter is not null)
                    if (await boundIter.MoveNextAsync())
                        yield return boundIter.Current;
            }
        }

        /// <inheritdoc />
        public IChannelWriter<TSignal> Writer<TSignal>()
        {
            if (typeof(TSignal) != Schema.Signal.Type)
                throw new ArgumentException($"Type {typeof(TSignal)} is not compatible with channel schema type {Schema.Signal}.");

            lock (this)
            {
                if (_writer is not null)
                    throw new InvalidOperationException("Only a single writer can be opened to a channel at a time.");
                if (_bound is not null)
                    throw new InvalidOperationException("A channel cannot be opened for write when it is bound to another channel.");

                var writer = new ChannelStoreWriter<TSignal>((IChannelStore<TSignal>)_store, ReleaseWriter);
                _writer = writer;
                return writer;
            }
        }

        /// <summary>
        /// Disposes of the writer.
        /// </summary>
        void ReleaseWriter()
        {
            lock (this)
            {
                _writer = null;
            }
        }

        /// <inheritdoc />
        public void Bind(IReadableChannelHandle source)
        {
            lock (this)
            {
                if (_writer is not null)
                    throw new InvalidOperationException("Only a single writer can be opened to a channel at a time.");

                // bind to source, and signal cancellation of existing readers
                _bound = source;
                _boundCancellationTokenSource.Cancel();
                _boundCancellationTokenSource = new CancellationTokenSource();
            }
        }


    }

}
