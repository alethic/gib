using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading;

using Gip.Abstractions;

namespace Gip.Base.Collections
{

    public static class SequenceChannelExtensions
    {

        /// <summary>
        /// Creates a new emitter for the 'sequence' protocol.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static SequenceEmitter<T> EmitSequence<T>(this IWritableChannelHandle channel) => new SequenceEmitter<T>(channel.OpenWrite<SequenceSignal<T>>());

        /// <summary>
        /// Parses the channel for the 'list' protocol, returning an enumeration of completed sets.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async IAsyncEnumerable<ImmutableList<T>> CollectSequence<T>(this IReadableChannelHandle channel, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var value = ImmutableList<T>.Empty;
            var pause = false;

            await foreach (var signal in channel.OpenRead<SequenceSignal<T>>(cancellationToken))
            {
                switch (signal)
                {
                    case SequenceFreezeSignal<T>:
                        pause = true;
                        break;
                    case SequenceResumeSignal<T>:
                        pause = false;
                        break;
                    case SequenceAppendSignal<T> appendEvent:
                        value = value.Add(appendEvent.Item);
                        break;
                    case SequenceAppendManySignal<T> appendManyEvent:
                        value = value.AddRange(appendManyEvent.Items);
                        break;
                    case SequenceClearSignal<T>:
                        value = value.Clear();
                        break;
                }

                if (pause == false)
                    yield return value;
            }
        }

    }

}
