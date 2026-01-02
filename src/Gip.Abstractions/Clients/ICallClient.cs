using System;

namespace Gip.Abstractions.Clients
{

    public interface ICallClient : IDisposable, IAsyncDisposable
    {

        /// <summary>
        /// Gets the channel client for the given indexed output.
        /// </summary>
        IChannelClient<T> GetOutputChannel<T>(int index);

    }

}
