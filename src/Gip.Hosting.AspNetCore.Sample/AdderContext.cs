using System.Threading;
using System.Threading.Tasks;

using Gip.Abstractions;
using Gip.Base;
using Gip.Core;

namespace Gip.Hosting.AspNetCore.Sample
{

    public class AdderContext : FunctionContextBase
    {

        /// <summary>
        /// Gets the schema for the function.
        /// </summary>
        public override FunctionSchema Schema { get; } = FunctionSchema.CreateBuilder()
            .Source<ValueSignal<int>>()
            .Source<ValueSignal<int>>()
            .Output<ValueSignal<int>>()
            .Build();

        /// <summary>
        /// Handles an individual call.
        /// </summary>
        /// <param name="call"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task CallAsync(ICallContext call, CancellationToken cancellationToken)
        {
            using var ret = call.Outputs[0].EmitValue<int>();
            await foreach (var (x, y) in AsyncEnumerableExtensions.Latest(
                call.Sources[0].CollectValue<int>(cancellationToken),
                call.Sources[1].CollectValue<int>(cancellationToken),
                cancellationToken))
            {
                ret.Set(x + y);
            }
        }

    }

}