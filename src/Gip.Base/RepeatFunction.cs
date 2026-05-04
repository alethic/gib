using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Gip.Abstractions;
using Gip.Base.Collections;
using Gip.Core;

namespace Gip.Base
{

    public class RepeatFunction<TKey> : FunctionContextBase
        where TKey : notnull
    {

        /// <summary>
        /// Gets the schema for the function.
        /// </summary>
        public override FunctionSchema Schema { get; } = FunctionSchema.CreateBuilder()
            .SourceValue<IFunctionHandle>("function")
            .SourceMap<TKey, ImmutableArray<IReadableChannelHandle>>("template")
            .OutputMap<TKey, ImmutableArray<IReadableChannelHandle>>("output")
            .Build();

        /// <summary>
        /// Handles the invocation.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task CallAsync(ICallContext context, CancellationToken cancellationToken)
        {
            using var output = context.Outputs[0].EmitMap<TKey, ImmutableArray<IReadableChannelHandle>>();

            // maintain our outstanding calls
            var cached = new Dictionary<TKey, (ICallHandle Handle, ImmutableArray<IReadableChannelHandle> Sources)>();

            // for each function handle
            await foreach (var func in context.Sources[0].CollectValue<IFunctionHandle>(cancellationToken))
            {
                // new function handle means we end all of our existing calls
                foreach (var call in cached)
                    await call.Value.Handle.DisposeAsync();

                // signal complete clear
                cached.Clear();
                output.Clear();

                // each time the template changes
                await foreach (var template in context.Sources[1].CollectMap<TKey, ImmutableArray<IReadableChannelHandle>>(cancellationToken))
                {
                    // for each item in the template
                    foreach (var (key, sources) in template)
                    {
                        // call does not exist, invoke and add
                        if (cached.TryGetValue(key, out var state) == false)
                        {
                            var call = await func.CallAsync(sources, cancellationToken);
                            cached.Add(key, (call, sources));
                            output.Add(key, call.Outputs);
                        }
                        else
                        {
                            // exists, check that existing source arguments match last
                            var (call, known) = state;
                            if (call.Function != func || sources.SequenceEqual(known) == false)
                            {
                                await call.DisposeAsync();
                                cached.Remove(key);
                                cached.Add(key, (await func.CallAsync(sources, cancellationToken), sources));
                                output.Freeze();
                                output.Remove(key);
                                output.Add(key, call.Outputs);
                                output.Resume();
                            }
                            else
                            {
                                // call already exists and is up to date
                            }
                        }
                    }
                }
            }
        }

    }

}
