using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

using Gip.Abstractions;

namespace Gip.Base
{

    public static class ValueChannelExtensions
    {

        /// <summary>
        /// Creates a new emitter for the 'value' protocol.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static ValueEmitter<T> EmitValue<T>(this IWritableChannelHandle channel) => new ValueEmitter<T>(channel.OpenWrite<ValueSignal<T>>());

        /// <summary>
        /// Parses the channel for the value protocol.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async IAsyncEnumerable<T> CollectValue<T>(this IReadableChannelHandle channel, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var first = true;
            var value = default(T?);

            await foreach (var signal in channel.OpenRead<ValueSignal<T>>(cancellationToken).OfType<SetValueSignal<T>>())
            {
                if (first || Equals(signal, value) == false)
                {
                    first = false;
                    value = signal.Value;
                    yield return value;
                }
            }
        }

    }

}
