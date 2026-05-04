using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Gip.Abstractions;
using Gip.Core;

namespace Gip.Base.Collections
{

    /// <summary>
    /// Generic function that maps one set of inputs to a set of outputs.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ConcatSequenceSet<T> : FunctionContextBase
    {

        /// <summary>
        /// Gets the schema for the function.
        /// </summary>
        public override FunctionSchema Schema { get; } = FunctionSchema.CreateBuilder()
            .SourceSet<IReadableChannelHandle>()
            .OutputSequence<T>()
            .Build();

        /// <summary>
        /// Handles an individual call.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task CallAsync(ICallContext context, CancellationToken cancellationToken)
        {
            using var output = context.Outputs[0].EmitSequence<T>();

            await foreach (var sources in context.Sources[0].CollectSet<IReadableChannelHandle>(cancellationToken))
            {
                output.Clear();

                var c = Channel.CreateBounded<ImmutableList<ReadOnlyMemory<T>>>(1);

                var l = new List<Task>();
                foreach (var source in sources)
                {
                    l.Add(Task.Run(async () =>
                    {
                        await foreach (var i in source.CollectSequence<T>(cancellationToken))
                            await c.Writer.WriteAsync(i, cancellationToken);
                    }));
                }

                await foreach (var i in c.Reader.ReadAllAsync(cancellationToken))
                    foreach (var m in i)
                        output.AppendRange(m);

                await Task.WhenAll(l);
            }
        }

    }

}
