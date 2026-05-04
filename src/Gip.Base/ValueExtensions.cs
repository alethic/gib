using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;

using Gip.Abstractions;

namespace Gip.Base
{

    public static class ValueExtensions
    {

        /// <summary>
        /// Creates a new emitter for the 'value' protocol.
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static ValueEmitter<T> EmitValue<T>(this IWritableChannelHandle channel) => new ValueEmitter<T>(channel.Writer<ValueSignal<T>>());

        /// <summary>
        /// Adds a source value to the schema.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static FunctionSchemaBuilder SourceValue<T>(this FunctionSchemaBuilder builder, string? name = null) => builder.Source<ValueSignal<T>>(name);

        /// <summary>
        /// Creates a channel that contains a single value.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="pipeline"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static ILocalChannelHandle CreateSingletonValue<T>(this IPipelineContext pipeline, T value)
        {
            var channel = pipeline.CreateChannel(ChannelSchema.FromClrType<ValueSignal<T>>());
            using var emitter = channel.EmitValue<T>();
            emitter.Set(value);
            return channel;
        }

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

            await foreach (var reader in channel.Reader<ValueSignal<T>>(cancellationToken))
            {
                await foreach (var signal in reader.OfType<SetValueSignal<T>>())
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

}
