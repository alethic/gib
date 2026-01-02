using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Gib.Core.Elements
{

    public interface IStreamProducer<T>
    {

        /// <summary>
        /// Signals that the stream state is to be reset, that is, we can ignore any previous events.
        /// </summary>
        /// <returns></returns>
        Task ResetAsync();

        /// <summary>
        /// Appends an event to the stream.
        /// </summary>
        /// <param name="event"></param>
        /// <returns></returns>
        Task SendAsync(T @event);

        /// <summary>
        /// Adds many events to the stream.
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        Task SendManyAsync(T[] events);

        /// <summary>
        /// Adds many events to the stream.
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        Task SendManyAsync(ImmutableArray<T> events);

        /// <summary>
        /// Adds many events to the stream.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        Task SendManyAsync(IReadOnlyList<T> items);

        /// <summary>
        /// Adds many events to the stream.
        /// </summary>
        /// <param name="events"></param>
        /// <returns></returns>
        Task SendManyAsync(ReadOnlySpan<T> events);

        /// <summary>
        /// Adds many events to the stream.
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        Task SendManyAsync(ReadOnlyMemory<T> items);

        /// <summary>
        /// Binds the producer to an existing stream.
        /// </summary>
        /// <param name="binding"></param>
        /// <returns></returns>
        Task BindAsync(IStreamBinding<T> binding);

    }

}
