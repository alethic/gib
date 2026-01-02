using System;
using System.Collections.Generic;

using Gip.Abstractions.Clients;

namespace Gip.Core.Clients
{

    public class ClientFactory : IClientFactory
    {

        readonly IEnumerable<IClientProtocol> _protocols;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="protocols"></param>
        public ClientFactory(IEnumerable<IClientProtocol> protocols)
        {
            _protocols = protocols;
        }

        /// <summary>
        /// Opens a client to the remote channel uri.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public IFunctionClient GetFunction(Uri uri)
        {
            foreach (var protocol in _protocols)
                if (protocol.TryGetFunction(uri, out var client))
                    return client;

            throw new InvalidOperationException();
        }

        /// <summary>
        /// Opens a client to the remote channel uri.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        public IChannelClient<T> GetChannel<T>(Uri uri)
        {
            foreach (var protocol in _protocols)
                if (protocol.TryGetChannel<T>(uri, out var client))
                    return client;

            throw new InvalidOperationException();
        }

    }

}
