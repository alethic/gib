using System;
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
        public static SequenceEmitter<T> EmitSequence<T>(this IWritableChannelHandle channel) => new SequenceEmitter<T>(channel.Writer<SequenceSignal<T>>());

        /// <summary>
        /// Adds a source sequence to the schema.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static FunctionSchemaBuilder SourceSequence<T>(this FunctionSchemaBuilder builder, string? name= null) => builder.Source<SequenceSignal<T>>(name);

        /// <summary>
        /// Adds a output sequence to the schema.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static FunctionSchemaBuilder OutputSequence<T>(this FunctionSchemaBuilder builder, string? name = null) => builder.Output<SequenceSignal<T>>(name);

        /// <summary>
        /// Parses the channel for the 'sequence' protocol, returning an enumeration of completed sequence.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="channel"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static async IAsyncEnumerable<ImmutableList<ReadOnlyMemory<T>>> CollectSequence<T>(this IReadableChannelHandle channel, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            await foreach (var reader in channel.Reader<SequenceSignal<T>>(cancellationToken))
            {
                var value = ImmutableList<ReadOnlyMemory<T>>.Empty;
                var pause = false;

                await foreach (var signal in reader)
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
                            value = value.Add(new T[] { appendEvent.Item });
                            break;
                        case SequenceAppendManySignal<T> appendManyEvent:
                            value = value.Add(appendManyEvent.Items);
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

}
