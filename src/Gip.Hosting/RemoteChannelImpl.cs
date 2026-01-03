using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Gip.Abstractions;
using Gip.Abstractions.Clients;

namespace Gip.Hosting
{

    /// <summary>
    /// Implementation of a <see cref="IReadableChannelHandle"/> that retrieves signals from a remote channel.
    /// </summary>
    class RemoteChannelImpl : IRemoteChannelHandle
    {

        readonly IClientFactory _clients;
        readonly ChannelSchema _schema;
        readonly Uri _uri;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="uri"></param>
        public RemoteChannelImpl(IClientFactory clients, ChannelSchema schema, Uri uri)
        {
            _clients = clients;
            _schema = schema;
            _uri = uri;
        }

        /// <inheritdoc />
        public ChannelSchema Schema => _schema;

        /// <inheritdoc />
        public Uri Uri=> _uri;

        /// <inheritdoc />
        public async IAsyncEnumerable<T> OpenRead<T>([EnumeratorCancellation] CancellationToken cancellationToken)
        {
            await using var c = _clients.GetChannel<T>(_uri);
            await foreach (var i in c.WithCancellation(cancellationToken))
                yield return i ?? throw new InvalidOperationException();
        }

    }

}
