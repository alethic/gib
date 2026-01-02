using System;
using System.Collections.Generic;
using System.Threading;

namespace Gip.Abstractions
{

    /// <summary>
    /// Holds a reference to a local channel.
    /// </summary>
    public interface IChannelHandle
    {

        /// <summary>
        /// Gets the ID of the channel.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the schema of the channel.
        /// </summary>
        ChannelSchema Schema { get; }

        /// <summary>
        /// Opens the channel for reading. Existing signals should be made available to the reader, as well as a feed of new signals.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IAsyncEnumerable<T> OpenAsync<T>(CancellationToken cancellationToken);

    }

}
