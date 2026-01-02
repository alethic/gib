using System;
using System.Collections.Generic;
using System.Threading;

using Gip.Abstractions;

namespace Gip.Hosting
{

    /// <summary>
    /// Holds a reference to a registered chanel in the channel container.
    /// </summary>
    public class LocalChannel : IChannelHandle
    {

        readonly LocalChannelContainer _container;
        readonly ChannelSchema _schema;
        readonly IChannelStore _store;
        readonly Guid _id;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="container"></param>
        /// <param name="store"></param>
        /// <param name="id"></param>
        internal LocalChannel(LocalChannelContainer container, ChannelSchema schema, IChannelStore store, Guid id)
        {
            _container = container;
            _schema = schema;
            _store = store;
            _id = id;
        }

        /// <summary>
        /// Gets the registered <see cref="IChannelStore"/>.
        /// </summary>
        public IChannelStore Store => _store;

        /// <summary>
        /// Gets the ID of the channel.
        /// </summary>
        public Guid Id => _id;

        /// <inheritdoc />
        public ChannelSchema Schema => _schema;

        /// <inheritdoc />
        public IAsyncEnumerable<T> OpenAsync<T>(CancellationToken cancellationToken) => ((IChannelStore<T>)_store).OpenAsync(cancellationToken);

    }

}