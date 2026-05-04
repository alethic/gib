using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using Gip.Abstractions;

namespace Gip.Base.Collections
{

    public static class ListExtensions
    {

        /// <summary>
        /// Adds a source list to the schema.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static FunctionSchemaBuilder SourceList<T>(this FunctionSchemaBuilder builder, string? name = null) => builder.Source<ListSignal<T>>(name);

        /// <summary>
        /// Adds a source list to the schema.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static FunctionSchemaBuilder OutputList<T>(this FunctionSchemaBuilder builder, string? name = null) => builder.Output<ListSignal<T>>(name);

        /// <summary>
        /// Creates a new emitter for the 'list' protocol.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static ListEmitter<T> EmitList<T>(this IWritableChannelHandle channel) => new ListEmitter<T>(channel.Writer<ListSignal<T>>());

        /// <summary>
        /// Parses the channel for the 'list' protocol, returning an enumeration of completed sets.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async IAsyncEnumerable<ImmutableList<T>> CollectList<T>(this IReadableChannelHandle channel, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var reader in channel.Reader<SetSignal<T>>(cancellationToken))
            {
                var value = ImmutableList<T>.Empty;
                var pause = false;

                await foreach (var signal in reader)
                {
                    switch (signal)
                    {
                        case ListFreezeSignal<T>:
                            pause = true;
                            break;
                        case ListResumeSignal<T>:
                            pause = false;
                            break;
                        case ListInsertSignal<T> insertEvent:
                            value = value.Insert(insertEvent.Index.GetOffset(value.Count), insertEvent.Item);
                            break;
                        case ListInsertManySignal<T> insertManyEvent:
                            value = value.InsertRange(insertManyEvent.Index.GetOffset(value.Count), insertManyEvent.Items);
                            break;
                        case ListRemoveSignal<T> removeEvent:
                            value = value.RemoveAt(removeEvent.Index.GetOffset(value.Count));
                            break;
                        case ListRemoveManySignal<T> removeManyEvent:
                            value = value.RemoveRange(removeManyEvent.Range.Start.GetOffset(value.Count), removeManyEvent.Range.End.GetOffset(value.Count) - removeManyEvent.Range.Start.GetOffset(value.Count));
                            break;
                        case ListSetSignal<T> setEvent:
                            value = value.SetItem(setEvent.Index.GetOffset(value.Count), setEvent.Item);
                            break;
                        case ListSetManySignal<T> setManyEvent:

                            switch (setManyEvent.Items.Length)
                            {
                                case > 1:
                                    var b = value.ToBuilder();
                                    for (int i = setManyEvent.Index.GetOffset(b.Count); i < setManyEvent.Items.Length - setManyEvent.Index.GetOffset(b.Count); i++)
                                        b[i] = setManyEvent.Items[i];

                                    value = b.ToImmutable();
                                    break;
                                case 1:
                                    value = value.SetItem(setManyEvent.Index.GetOffset(value.Count), setManyEvent.Items[0]);
                                    break;
                            }

                            break;
                        case ListClearSignal<T>:
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
