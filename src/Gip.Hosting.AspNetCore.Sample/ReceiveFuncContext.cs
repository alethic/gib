using System;
using System.Threading;
using System.Threading.Tasks;

using Gip.Abstractions;
using Gip.Base;
using Gip.Core;

namespace Gip.Hosting.AspNetCore.Sample
{

    public class ReceiveFuncContext : FunctionContextBase
    {

        /// <summary>
        /// Gets the schema for the function.
        /// </summary>
        public override FunctionSchema Schema { get; } = FunctionSchema.CreateBuilder()
            .Source<ValueSignal<FunctionReference>>()
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
            ICallHandle? opCall = null;

            await foreach (var opRef in call.Sources[0].CollectValue<FunctionReference>())
            {
                if (opCall is not null)
                {
                    await opCall.DisposeAsync();
                    opCall = null;
                }

                var opFunc = call.Pipeline.ResolveFunction(opRef);
                Console.WriteLine("Switching operation to {0}", call.Pipeline.GetFunctionReference(opFunc).Uri);
                opCall = await opFunc.CallAsync([call.Sources[1], call.Sources[2]], [call.Outputs[0]], cancellationToken);
            }

            // wait for any outstanding call to terminate
            if (opCall is not null)
            {
                await opCall.DisposeAsync();
                opCall = null;
            }
        }

    }

}