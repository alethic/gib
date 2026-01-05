using System;
using System.Collections.Generic;
using System.Threading;

using Gip.Abstractions;

namespace Gip.Hosting
{

    /// <summary>
    /// Holds a reference to a registered chanel in the channel container.
    /// </summary>
    class ChannelImpl : ILocalChannelHandle
    {

        readonly ChannelSchema _schema;
        readonly IChannelStore _store;
        readonly Guid _id;

        IChannelWriter? _writer = null;

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
        public IAsyncEnumerable<T> OpenRead<T>(CancellationToken cancellationToken)
        {
            if (typeof(T) != Schema.Signal.Type)
                throw new ArgumentException($"Type {typeof(T)} is not compatible with channel schema type {Schema.Signal}.");

            return ((IChannelStore<T>)_store).OpenAsync(cancellationToken);
        }

        /// <inheritdoc />
        public IChannelWriter<T> OpenWrite<T>()
        {
            if (typeof(T) != Schema.Signal.Type)
                throw new ArgumentException($"Type {typeof(T)} is not compatible with channel schema type {Schema.Signal}.");

            lock (this)
            {
                if (_writer is not null)
                    throw new InvalidOperationException("Only a single writer can be opened to a channel at a time.");

                var writer = new ChannelStoreWriter<T>((IChannelStore<T>)_store, ReleaseWriter);
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

    }

}
