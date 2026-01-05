using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading;

using Gip.Abstractions;

namespace Gip.Base.Collections
{

    public static class SetChannelExtensions
    {

        /// <summary>
        /// Creates a new emitter for the 'set' protocol.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static SetEmitter<T> EmitSet<T>(this IWritableChannelHandle channel) => new SetEmitter<T>(channel.OpenWrite<SetSignal<T>>());

        /// <summary>
        /// Parses the channel for the 'set' protocol, returning an enumeration of completed sets.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async IAsyncEnumerable<ImmutableHashSet<T>> CollectSet<T>(this IReadableChannelHandle channel, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var value = ImmutableHashSet<T>.Empty;
            var pause = false;

            await foreach (var signal in channel.OpenRead<SetSignal<T>>(cancellationToken))
            {
                switch (signal)
                {
                    case SetFreezeSignal<T>:
                        pause = true;
                        break;
                    case SetResumeSignal<T>:
                        pause = false;
                        break;
                    case SetAddSignal<T> addEvent:
                        value = value.Add(addEvent.Item);
                        break;
                    case SetAddManySignal<T> addManyEvent:
                        value = value.Union(addManyEvent.Items);
                        break;
                    case SetRemoveSignal<T> removeEvent:
                        value = value.Remove(removeEvent.Item);
                        break;
                    case SetRemoveManySignal<T> removeManyEvent:
                        value = value.Except(removeManyEvent.Items);
                        break;
                    case SetClearSignal<T>:
                        value = value.Clear();
                        break;
                }

                if (pause == false)
                    yield return value;
            }
        }

    }

}
