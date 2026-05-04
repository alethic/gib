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
    /// <typeparam name="TResult"></typeparam>
    public class DelegateListMap<TSource, TResult> : FunctionContextBase
    {

        readonly Func<ICallContext, TSource, CancellationToken, ValueTask<TResult>> _func;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="func"></param>
        public DelegateListMap(Func<ICallContext, TSource, CancellationToken, ValueTask<TResult>> func)
        {
            _func = func;
        }

        /// <summary>
        /// Gets the schema for the function.
        /// </summary>
        public override FunctionSchema Schema { get; } = FunctionSchema.CreateBuilder()
            .Source<ListSignal<TSource>>()
            .Output<ListSignal<TResult>>()
            .Build();

        /// <summary>
        /// Handles an individual call.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task CallAsync(ICallContext context, CancellationToken cancellationToken)
        {
            using var output = context.Outputs[0].EmitList<TResult>();

            // each time the template changes
            await foreach (var source in context.Sources[0].Reader<ListSignal<TSource>>(cancellationToken))
            {
                output.Clear();

                await foreach (var signal in source)
                {
                    switch (signal)
                    {
                        case ListFreezeSignal<TSource>:
                            output.Freeze();
                            break;
                        case ListResumeSignal<TSource>:
                            output.Resume();
                            break;
                        case ListInsertSignal<TSource> insertSignal:
                            output.Insert(insertSignal.Index, await _func(context, insertSignal.Item, cancellationToken));
                            break;
                        case ListInsertManySignal<TSource> insertManySignal:
                            output.InsertRange(insertManySignal.Index, await insertManySignal.Items.ToAsyncEnumerable().Select(async (item, ct) => await _func(context, item, ct)).ToArrayAsync(cancellationToken));
                            break;
                        case ListRemoveSignal<TSource> removeSignal:
                            output.Remove(removeSignal.Index);
                            break;
                        case ListRemoveManySignal<TSource> removeManySignal:
                            output.RemoveRange(removeManySignal.Range);
                            break;
                        case ListSetSignal<TSource> setSignal:
                            output.Set(setSignal.Index, await _func(context, setSignal.Item, cancellationToken));
                            break;
                        case ListSetManySignal<TSource> setManySignal:
                            output.InsertRange(setManySignal.Index, await setManySignal.Items.ToAsyncEnumerable().Select(async (item, ct) => await _func(context, item, ct)).ToArrayAsync(cancellationToken));
                            break;
                        case ListClearSignal<TSource>:
                            output.Clear();
                            break;
                    }
                }
            }
        }

    }

}
