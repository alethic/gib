using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Gib.Core.Elements
{

    public interface IStreamConsumer<T> : IAsyncEnumerable<T>
    {

        /// <summary>
        /// Invokes the given action with the complete stream on each change.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        Task Listen(Action<ReadOnlySequence<T>> action, CancellationToken cancellationToken);

        /// <summary>
        /// Invokes the action for each event that arrives on the stream.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        Task Stream(Action<T> action, CancellationToken cancellationToken);

    }

}
