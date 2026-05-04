using System;
using System.Collections.Generic;

namespace Gip.Abstractions
{

    /// <summary>
    /// Provides an interface for writing to an output channel.
    /// </summary>
    public interface IChannelReader : IDisposable
    {

        /// <summary>
        /// Gets the channel being read. In the case of a redirect this might refer to a different channel that the
        /// reader was initially open on.
        /// </summary>
        IReadableChannelHandle Channel { get; }

    }
    
    /// <summary>
    /// Provides an interface for reading from an output channel.
    /// </summary>
    /// <typeparam name="TSignal"></typeparam>
    public interface IChannelReader<TSignal> : IChannelReader, IAsyncEnumerable<TSignal>
    {



    }

}
