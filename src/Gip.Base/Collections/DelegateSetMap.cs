using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Gip.Abstractions;
using Gip.Core;

namespace Gip.Base.Collections
{

    /// <summary>
    /// Generic function that maps one set of inputs to a set of outputs.
    /// </summary>
    /// <typeparam name="TSource"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    public class DelegateSetMap<TSource, TResult> : FunctionContextBase
        where TSource : notnull
    {

        readonly Func<ICallContext, TSource, CancellationToken, ValueTask<TResult>> _func;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="func"></param>
        public DelegateSetMap(Func<ICallContext, TSource, CancellationToken, ValueTask<TResult>> func)
        {
            _func = func;
        }

        /// <summary>
        /// Gets the schema for the function.
        /// </summary>
        public override FunctionSchema Schema { get; } = FunctionSchema.CreateBuilder()
            .Source<SetSignal<TSource>>()
            .Output<SetSignal<TResult>>()
            .Build();

        /// <summary>
        /// Handles an individual call.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task CallAsync(ICallContext context, CancellationToken cancellationToken)
        {
            using var output = context.Outputs[0].EmitSet<TResult>();

            // each time the template changes
            await foreach (var source in context.Sources[0].Reader<SetSignal<TSource>>(cancellationToken))
            {
                var cached = new Dictionary<TSource, TResult>();
                output.Clear();

                await foreach (var signal in source)
                {
                    switch (signal)
                    {
                        case SetFreezeSignal<TSource>:
                            output.Freeze();
                            break;
                        case SetResumeSignal<TSource>:
                            output.Resume();
                            break;
                        case SetAddSignal<TSource> addSignal:
                            {
                                var result = await _func(context, addSignal.Item, cancellationToken);
                                cached.Add(addSignal.Item, result);
                                output.Add(result);
                                break;
                            }
                        case SetAddManySignal<TSource> addManySignal:
                            {
                                var results = new List<TResult>(addManySignal.Items.Length);
                                foreach (var item in addManySignal.Items)
                                {
                                    var result = await _func(context, item, cancellationToken);
                                    if (cached.TryAdd(item, result))
                                        results.Add(result);
                                }

                                output.AddRange(results);
                                break;
                            }
                        case SetRemoveSignal<TSource> removeSignal:
                            {
                                if (cached.TryGetValue(removeSignal.Item, out var result))
                                {
                                    cached.Remove(removeSignal.Item);
                                    output.Remove(result);
                                }

                                break;
                            }
                        case SetRemoveManySignal<TSource> removeManySignal:
                            {
                                var l = new List<TResult>(removeManySignal.Items.Length);
                                foreach (var item in removeManySignal.Items)
                                {
                                    if (cached.TryGetValue(item, out var result))
                                    {
                                        cached.Remove(item);
                                        l.Add(result);
                                    }
                                }

                                output.RemoveRange(l);
                                break;
                            }
                        case SetClearSignal<TSource>:
                            output.Clear();
                            break;
                    }
                }
            }
        }

    }

}
