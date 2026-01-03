using System;
using System.Diagnostics.CodeAnalysis;

using Gip.Abstractions;
using Gip.Abstractions.Clients;

namespace Gip.Hosting
{

    /// <summary>
    /// Default <see cref="IPipelineContext"/> implementation.
    /// </summary>
    public abstract class Pipeline : IPipelineContext
    {

        readonly IServiceProvider _serviceProvider;
        readonly FunctionContainer _functions;
        readonly ChannelContainer _channels;
        readonly IClientFactory _clients;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="clients"></param>
        public Pipeline(IServiceProvider serviceProvider, IClientFactory clients)
        {
            _serviceProvider = serviceProvider;
            _clients = clients;
            _functions = new(this);
            _channels = new(this);
        }

        /// <summary>
        /// Gets the service provider.
        /// </summary>
        public IServiceProvider ServiceProvider => _serviceProvider;

        /// <summary>
        /// Attempts to get a reference to an existing function.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="registration"></param>
        /// <returns></returns>
        public bool TryGetFunction(Guid id, [NotNullWhen(true)] out ILocalFunctionHandle? registration)
        {
            if (_functions.TryGetFunction(id, out var h))
            {
                registration = h;
                return true;
            }

            registration = null;
            return false;
        }

        /// <summary>
        /// Attempts to get a reference to an existing function.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="registration"></param>
        /// <returns></returns>
        public bool TryGetChannel(Guid id, [NotNullWhen(true)] out ILocalChannelHandle? registration)
        {
            if (_channels.TryGetChannel(id, out var h))
            {
                registration = h;
                return true;
            }

            registration = null;
            return false;
        }

        /// <summary>
        /// Registers a channel for the given schema.
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        public ILocalFunctionHandle CreateFunction(IFunctionContext function)
        {
            return _functions.Create(function);
        }

        /// <summary>
        /// Registers a channel for the given schema.
        /// </summary>
        /// <param name="schema"></param>
        /// <returns></returns>
        public ILocalChannelHandle CreateChannel(ChannelSchema schema)
        {
            return _channels.Create(schema);
        }

        /// <inheritdoc />
        public Uri GetFunctionUri(IFunctionHandle function)
        {
            if (function is ILocalFunctionHandle local)
                return GetLocalFunctionUri(local.Id);
            else if (function is IRemoteFunctionHandle remote)
                return remote.Uri;
            else
                throw new NotImplementedException();
        }

        /// <inheritdoc />
        public Uri GetChannelUri(IChannelHandle channel)
        {
            if (channel is ILocalChannelHandle local)
                return GetLocalChannelUri(local.Id);
            else if (channel is IRemoteChannelHandle remote)
                return remote.Uri;
            else
                throw new NotImplementedException();
        }

        /// <inheritdoc />
        public abstract Uri GetLocalFunctionUri(Guid functionId);

        /// <inheritdoc />
        public abstract Uri GetLocalChannelUri(Guid channelId);

        /// <inheritdoc />
        public IRemoteFunctionHandle GetRemoteFunction(Uri uri, FunctionSchema schema)
        {
            return new RemoteFunctionImpl(_clients, schema, uri);
        }

        /// <inheritdoc />
        public IRemoteChannelHandle GetRemoteChannel(Uri uri, ChannelSchema schema)
        {
            return new RemoteChannelImpl(_clients, schema, uri);
        }

        /// <inheritdoc />
        public abstract bool TryGetLocalFunctionId(Uri uri, out Guid functionId);

        /// <inheritdoc />
        public abstract bool TryGetLocalChannelId(Uri uri, out Guid channelId);

    }

}
