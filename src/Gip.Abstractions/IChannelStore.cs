using System.Collections.Generic;
using System.Threading;

namespace Gip.Abstractions
{

    /// <summary>
    /// Represents an interface to a local channels storage.
    /// </summary>
    public interface IChannelStore
    {



    }

    /// <summary>
    /// Represents an interface to a local channels storage.
    /// </summary>
    public interface IChannelStore<T> : IChannelStore
    {

        /// <summary>
        /// Stores a new signal in the channel.
        /// </summary>
        /// <param name="signal"></param>
        void Store(T signal);

        /// <summary>
        /// Resets the state of the channel.
        /// </summary>
        void Reset();

        /// <summary>
        /// Completes the channel.
        /// </summary>
        void Complete();

        /// <summary>
        /// Gets whether or not the channel has been completed.
        /// </summary>
        bool IsComplete { get; }

        /// <summary>
        /// Opens the channel for reading. Existing signals should be made available to the reader, as well as a feed of new signals.
        /// </summary>
        /// <returns></returns>
        IAsyncEnumerable<T> OpenAsync(CancellationToken cancellationToken);

    }

}
