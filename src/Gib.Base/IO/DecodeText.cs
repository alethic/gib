using System;
using System.Buffers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Gip.Abstractions;
using Gip.Base;
using Gip.Base.Collections;
using Gip.Core;

namespace Gib.Base.IO
{

    public class DecodeText : FunctionContextBase
    {

        /// <summary>
        /// Implementation of <see cref="ReadOnlySequenceSegment{T}"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        class MemorySegment<T> : ReadOnlySequenceSegment<T>
        {

            public MemorySegment(ReadOnlyMemory<T> memory)
            {
                Memory = memory;
            }

            public MemorySegment<T> Append(ReadOnlyMemory<T> memory)
            {
                var segment = new MemorySegment<T>(memory) { RunningIndex = RunningIndex + Memory.Length };
                Next = segment;
                return segment;
            }

        }

        /// <summary>
        /// Gets the schema for the function.
        /// </summary>
        public override FunctionSchema Schema { get; } = FunctionSchema.CreateBuilder()
            .Source<SequenceSignal<byte>>()
            .Source<ValueSignal<string>>()
            .Output<ValueSignal<string>>()
            .Output<SequenceSignal<string>>()
            .Build();

        /// <summary>
        /// Handles an individual call.
        /// </summary>
        /// <param name="call"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task CallAsync(ICallContext call, CancellationToken cancellationToken)
        {
            var textData = call.Outputs[0].EmitValue<string?>();
            var messages = call.Outputs[1].EmitSequence<string>();

            await foreach (var (byteData, encodingName) in AsyncEnumerableExtensions.Latest(
                call.Sources[0].CollectSequence<byte>(cancellationToken),
                call.Sources[1].CollectValue<string>(cancellationToken),
                cancellationToken))
            {
                textData.Set(null);
                messages.Clear();

                Encoding encoding;
                try
                {
                    encoding = Encoding.GetEncoding(encodingName);
                }
                catch (ArgumentException e)
                {
                    messages.Append(e.Message);
                    continue;
                }

                try
                {
                    // prepare memory segments
                    var first = new MemorySegment<byte>(default);
                    var final = first;
                    foreach (var i in byteData)
                        final = final.Append(i);

                    // decode data
                    textData.Set(encoding.GetString(new ReadOnlySequence<byte>(first, 0, final, final.Memory.Length)));
                }
                catch (DecoderFallbackException e)
                {
                    messages.Append(e.Message);
                    continue;
                }
            }
        }

    }

}
