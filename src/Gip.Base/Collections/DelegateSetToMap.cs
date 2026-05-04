using System;
using System.Linq;
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
    /// <typeparam name="TKey"></typeparam>
    /// <typeparam name="TValue"></typeparam>
    public class DelegateSetToMap<TSource, TKey, TValue> : FunctionContextBase
        where TKey : notnull
    {

        readonly Func<ICallContext, TSource, CancellationToken, ValueTask<TKey>> _keyFunc;
        readonly Func<ICallContext, TSource, CancellationToken, ValueTask<TValue>> _valueFunc;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="func"></param>
        public DelegateSetToMap(Func<ICallContext, TSource, CancellationToken, ValueTask<TKey>> keyFunc, Func<ICallContext, TSource, CancellationToken, ValueTask<TValue>> valueFunc)
        {
            _keyFunc = keyFunc;
            _valueFunc = valueFunc;
        }

        /// <summary>
        /// Gets the schema for the function.
        /// </summary>
        public override FunctionSchema Schema { get; } = FunctionSchema.CreateBuilder()
            .Source<SetSignal<TSource>>()
            .Output<MapSignal<TKey, TValue>>()
            .Build();

        /// <summary>
        /// Handles an individual call.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task CallAsync(ICallContext context, CancellationToken cancellationToken)
        {
            using var output = context.Outputs[0].EmitMap<TKey, TValue>();

            // each time the source changes
            await foreach (var source in context.Sources[0].Reader<SetSignal<TSource>>(cancellationToken))
            {
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
                            output.Add(await _keyFunc(context, addSignal.Item, cancellationToken), await _valueFunc(context, addSignal.Item, cancellationToken));
                            break;
                        case SetAddManySignal<TSource> addManySignal:
                            output.AddRange(await addManySignal.Items.ToAsyncEnumerable().ToDictionaryAsync((p, ct) => _keyFunc(context, p, ct), (p, ct) => _valueFunc(context, p, ct), null, cancellationToken));
                            break;
                        case SetRemoveSignal<TSource> removeSignal:
                            output.Remove(await _keyFunc(context, removeSignal.Item, cancellationToken));
                            break;
                        case SetRemoveManySignal<TSource> removeManySignal:
                            output.RemoveRange(await removeManySignal.Items.ToAsyncEnumerable().Select((p, ct) => _keyFunc(context, p, ct)).ToArrayAsync(cancellationToken));
                            break;
                        case SetClearSignal<TSource>:
                            output.Clear();
                            break;
                    }
                }
            }
        }

    }

}
