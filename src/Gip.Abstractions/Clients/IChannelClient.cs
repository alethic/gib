using System;
using System.Collections.Generic;

namespace Gip.Abstractions.Clients
{

    /// <summary>
    /// A <see cref="IChannelClient{T}"/> represents a connection to a remote Gip channel.
    /// </summary>
    /// <typeparam name="TSignal"></typeparam>
    public interface IChannelClient<TSignal> : IAsyncEnumerable<ChannelEvent<TSignal>>, IDisposable, IAsyncDisposable
    {



    }

}
