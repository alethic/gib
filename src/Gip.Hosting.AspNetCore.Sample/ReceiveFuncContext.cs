using System;
using System.Threading;
using System.Threading.Tasks;

using Gip.Abstractions;
using Gip.Core;

namespace Gip.Hosting.AspNetCore.Sample
{

    public class ReceiveFuncContext : FunctionContextBase
    {

        /// <summary>
        /// Gets the schema for the function.
        /// </summary>
        public override FunctionSchema Schema { get; } = new FunctionSchema(
            [
                new ChannelSchema(typeof(FunctionReference)),
                new ChannelSchema(typeof(int)),
                new ChannelSchema(typeof(int)),
            ],
            [
                new ChannelSchema(typeof(int)),
            ]
        );

        /// <summary>
        /// Handles an individual call.
        /// </summary>
        /// <param name="call"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task CallAsync(ICallContext call, CancellationToken cancellationToken)
        {
            var oArg = call.Sources[0];
            var xArg = call.Sources[1];
            var yArg = call.Sources[2];
            var ret = call.Outputs[0];

            ICallHandle? opCall = null;

            async ValueTask Eval(FunctionReference opRef, CancellationToken cancellationToken)
            {
                if (opCall is not null)
                {
                    await opCall.DisposeAsync();
                    opCall = null;
                }

                var opFunc = call.Pipeline.GetFunction(opRef);
                Console.WriteLine("Switching operation to {0}", call.Pipeline.GetFunctionReference(opFunc).Uri);
                opCall = await opFunc.CallAsync(call.Services, [xArg, yArg], [ret], cancellationToken);
            }

            // listen to all of our parameter subscriptions until the invocation is canceled
            await Task.WhenAll(ForEachAsync(oArg.OpenRead<FunctionReference>(cancellationToken), Eval, cancellationToken));

            // wait for any outstanding call to terminate
            if (opCall is not null)
            {
                await opCall.DisposeAsync();
                opCall = null;
            }
        }

    }

}