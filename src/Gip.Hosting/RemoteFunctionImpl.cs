using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Gip.Abstractions;
using Gip.Abstractions.Clients;

namespace Gip.Hosting
{

    /// <summary>
    /// Implementation of a <see cref="IRemoteFunctionHandle"/> that retrieves signals from a remote channel.
    /// </summary>
    class RemoteFunctionImpl : IRemoteFunctionHandle
    {

        readonly IClientFactory _clients;
        readonly FunctionSchema _schema;
        readonly Uri _uri;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="uri"></param>
        public RemoteFunctionImpl(IClientFactory clients, FunctionSchema schema, Uri uri)
        {
            _clients = clients;
            _schema = schema;
            _uri = uri;
        }

        /// <inheritdoc />
        public FunctionSchema Schema => _schema;

        /// <inheritdoc />
        public Uri Uri => _uri;

        /// <inheritdoc />
        public ValueTask<ICallHandle> CallAsync(ImmutableArray<IReadableChannelHandle?> sources, ImmutableArray<IWritableChannelHandle?> outputs, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

    }

}
