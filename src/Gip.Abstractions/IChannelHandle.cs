using System.Collections.Generic;
using System.Threading;

namespace Gip.Abstractions
{

    public interface IChannelHandle
    {

        /// <summary>
        /// Gets the schema of the channel.
        /// </summary>
        ChannelSchema Schema { get; }

        /// <summary>
        /// Opens the channel for reading. Existing signals should be made available to the reader, as well as a feed of new signals.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IAsyncEnumerable<T> OpenRead<T>(CancellationToken cancellationToken);

        /// <summary>
        /// Opens a channel for writing.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IChannelWriter<T> OpenWrite<T>();

    }

}
