using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading;

using Gip.Abstractions;

namespace Gip.Base.Collections
{

    public static class MapExtensions
    {

        /// <summary>
        /// Creates a new emitter for the 'map' protocol.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static MapEmitter<TKey, TValue> EmitMap<TKey, TValue>(this IWritableChannelHandle channel)
            where TKey : notnull
            => new MapEmitter<TKey, TValue>(channel.Writer<MapSignal<TKey, TValue>>());

        /// <summary>
        /// Adds a source map to the schema.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="builder"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static FunctionSchemaBuilder SourceMap<TKey, TValue>(this FunctionSchemaBuilder builder, string? name = null)
            where TKey : notnull
            => builder.Source<MapSignal<TKey, TValue>>(name);

        /// <summary>
        /// Adds a source map to the schema.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="builder"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static FunctionSchemaBuilder OutputMap<TKey, TValue>(this FunctionSchemaBuilder builder, string? name = null)
            where TKey : notnull
            => builder.Output<MapSignal<TKey, TValue>>(name);

        /// <summary>
        /// Parses the channel for the 'map' protocol, returning an enumeration of completed sets.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="channel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async IAsyncEnumerable<ImmutableDictionary<TKey, TValue>> CollectMap<TKey, TValue>(this IReadableChannelHandle channel, [EnumeratorCancellation] CancellationToken cancellationToken = default)
            where TKey : notnull
        {
            await foreach (var reader in channel.Reader<MapSignal<TKey, TValue>>(cancellationToken))
            {
                var value = ImmutableDictionary<TKey, TValue>.Empty;
                var pause = false;

                await foreach (var signal in reader)
                {
                    switch (signal)
                    {
                        case MapFreezeSignal<TKey, TValue>:
                            pause = true;
                            break;
                        case MapResumeSignal<TKey, TValue>:
                            pause = false;
                            break;
                        case MapPutSignal<TKey, TValue> putSignal:
                            value = value.Add(putSignal.Key, putSignal.Value);
                            break;
                        case MapPutManySignal<TKey, TValue> putManySignal:
                            value = value.AddRange(putManySignal.Items);
                            break;
                        case MapRemoveSignal<TKey, TValue> removeSignal:
                            value = value.Remove(removeSignal.Key);
                            break;
                        case MapRemoveManySignal<TKey, TValue> removeManySignal:
                            value = value.RemoveRange(removeManySignal.Keys);
                            break;
                        case MapClearSignal<TKey, TValue>:
                            value = value.Clear();
                            break;
                    }

                    if (pause == false)
                        yield return value;
                }
            }
        }

    }

}
