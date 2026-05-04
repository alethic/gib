using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Gip.Abstractions;
using Gip.Base.Collections;
using Gip.Core;

namespace Gip.Base
{

    /// <summary>
    /// Generic function that maps one map of inputs to a map of outputs.
    /// </summary>
    /// <typeparam name="TSourceKey"></typeparam>
    /// <typeparam name="TSourceValue"></typeparam>
    /// <typeparam name="TResultKey"></typeparam>
    /// <typeparam name="TResultValue"></typeparam>
    public class DelegateMapMap<TSourceKey, TSourceValue, TResultKey, TResultValue> : FunctionContextBase
        where TSourceKey : notnull
        where TResultKey : notnull
    {

        readonly Func<ICallContext, TSourceKey, CancellationToken, ValueTask<TResultKey>> _keyFunc;
        readonly Func<ICallContext, TSourceValue, CancellationToken, ValueTask<TResultValue>> _valueFunc;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="func"></param>
        public DelegateMapMap(Func<ICallContext, TSourceKey, CancellationToken, ValueTask<TResultKey>> keyFunc, Func<ICallContext, TSourceValue, CancellationToken, ValueTask<TResultValue>> valueFunc)
        {
            _keyFunc = keyFunc;
            _valueFunc = valueFunc;
        }

        /// <summary>
        /// Gets the schema for the function.
        /// </summary>
        public override FunctionSchema Schema { get; } = FunctionSchema.CreateBuilder()
            .Source<MapSignal<TSourceKey, TSourceValue>>()
            .Output<MapSignal<TResultKey, TResultValue>>()
            .Build();

        /// <summary>
        /// Handles an individual call.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task CallAsync(ICallContext context, CancellationToken cancellationToken)
        {
            using var output = context.Outputs[0].EmitMap<TResultKey, TResultValue>();

            // each time the template changes
            await foreach (var source in context.Sources[0].Reader<MapSignal<TSourceKey, TSourceValue>>(cancellationToken))
            {
                var cached = new Dictionary<TSourceKey, TResultKey>();
                output.Clear();

                await foreach (var signal in source)
                {
                    switch (signal)
                    {
                        case MapFreezeSignal<TSourceKey, TSourceValue>:
                            output.Freeze();
                            break;
                        case MapResumeSignal<TSourceKey, TSourceValue>:
                            output.Resume();
                            break;
                        case MapPutSignal<TSourceKey, TSourceValue> putSignal:
                            {
                                var resultKey = await _keyFunc(context, putSignal.Key, cancellationToken);
                                cached.Add(putSignal.Key, resultKey);
                                output.Add(resultKey, await _valueFunc(context, putSignal.Value, cancellationToken));
                                break;
                            }
                        case MapPutManySignal<TSourceKey, TSourceValue> putManySignal:
                            {
                                var l = new Dictionary<TResultKey, TResultValue>(putManySignal.Items.Count);
                                foreach (var item in putManySignal.Items)
                                {
                                    var resultKey = await _keyFunc(context, item.Key, cancellationToken);
                                    if (cached.TryAdd(item.Key, resultKey))
                                        l.Add(resultKey, await _valueFunc(context, item.Value, cancellationToken));
                                }

                                output.AddRange(l);
                                break;
                            }
                        case MapRemoveSignal<TSourceKey, TSourceValue> removeSignal:
                            {
                                if (cached.TryGetValue(removeSignal.Key, out var resultKey))
                                {
                                    cached.Remove(removeSignal.Key);
                                    output.Remove(resultKey);
                                }

                                break;
                            }
                        case MapRemoveManySignal<TSourceKey, TSourceValue> removeManySignal:
                            {
                                var l = new List<TResultKey>(removeManySignal.Keys.Length);
                                foreach (var item in removeManySignal.Keys)
                                {
                                    if (cached.TryGetValue(item, out var resultKey))
                                    {
                                        cached.Remove(item);
                                        l.Add(resultKey);
                                    }
                                }

                                output.RemoveRange(l);
                                break;
                            }
                        case MapClearSignal<TSourceKey, TSourceValue>:
                            output.Clear();
                            break;
                    }
                }
            }
        }

    }

}
