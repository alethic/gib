using System.Collections.Generic;
using System.Threading;

namespace Gip.Abstractions
{

    public interface IReadableChannelHandle : IChannelHandle
    {

        /// <summary>
        /// Opens the channel for reading. Existing signals should be made available to the reader, as well as a feed of new signals.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IAsyncEnumerable<T> OpenRead<T>(CancellationToken cancellationToken);

    }

}
