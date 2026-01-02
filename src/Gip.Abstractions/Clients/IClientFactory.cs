using System;

namespace Gip.Abstractions.Clients
{

    /// <summary>
    /// Provides an interface to retrieve function and channel clients for URIs.
    /// </summary>
    public interface IClientFactory
    {

        /// <summary>
        /// Gets a <see cref="IFunctionClient"/> that can interact with the given remote function.
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        IFunctionClient GetFunction(Uri uri);

        /// <summary>
        /// Gets a <see cref="IChannelClient{T}"/> that can interact with the given remote channel.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="uri"></param>
        /// <returns></returns>
        IChannelClient<T> GetChannel<T>(Uri uri);

    }

}