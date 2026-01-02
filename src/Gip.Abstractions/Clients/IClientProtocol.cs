using System;
using System.Diagnostics.CodeAnalysis;

namespace Gip.Abstractions.Clients
{

    public interface IClientProtocol
    {

        /// <summary>
        /// Attempts to get a function client for the given URI.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        bool TryGetFunction(Uri uri, [NotNullWhen(true)] out IFunctionClient? client);

        /// <summary>
        /// Attempts to get a channel client for the given URI.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <returns></returns>
        bool TryGetChannel<T>(Uri uri, [NotNullWhen(true)] out IChannelClient<T>? client);

    }

}
