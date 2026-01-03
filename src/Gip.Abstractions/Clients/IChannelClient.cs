using System;
using System.Collections.Generic;

namespace Gip.Abstractions.Clients
{

    /// <summary>
    /// A <see cref="IChannelClient{T}"/> represents a connection to a remote Gip channel.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IChannelClient<T> : IAsyncEnumerable<T>, IDisposable, IAsyncDisposable
    {



    }

}
