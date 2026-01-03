using System;
using System.Threading;
using System.Threading.Tasks;

using Gip.Abstractions;
using Gip.Core;

namespace Gip.Hosting.AspNetCore.Sample
{

    public class RootContext : FunctionContextBase
    {

        /// <summary>
        /// Gets the schema for the function.
        /// </summary>
        public override FunctionSchema Schema { get; } = new FunctionSchema(
            [

            ],
            [

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
            int i = 0;

            // function to receive reference to operator func
            var recvFunc = call.Pipeline.CreateFunction(new ReceiveFuncContext());

            // channel to send input to receive function
            var opChan = call.Pipeline.CreateChannel(new ChannelSchema(typeof(FunctionReference)));
            var xChan = call.Pipeline.CreateChannel(new ChannelSchema(typeof(int)));
            var yChan = call.Pipeline.CreateChannel(new ChannelSchema(typeof(int)));

            // channel on which to receive results from op
            var opResultChan = call.Pipeline.CreateChannel(new ChannelSchema(typeof(int)));

            // initiate call to receive function, which receives the operator, and writes the results to the result chan
            using var opCall = await recvFunc.CallAsync([opChan, xChan, yChan], [opResultChan], cancellationToken);

            // writer to send function reference value to receiver
            using var opWriter = opChan.OpenWrite<FunctionReference>();

            // channel and writer to write X value
            using var xWriter = xChan.OpenWrite<int>();

            // channel and writer to write Y value
            using var yWriter = yChan.OpenWrite<int>();

            // periodically change operator func
            var fun = Task.Run(async () =>
            {
                while (cancellationToken.IsCancellationRequested == false)
                {
                    opWriter.Reset();

                    if (i++ % 2 == 0)
                        opWriter.Write(call.Pipeline.GetFunctionReference(call.Pipeline.CreateFunction(new AdderContext())));
                    else
                        opWriter.Write(call.Pipeline.GetFunctionReference(call.Pipeline.CreateFunction(new MultiContext())));

                    await Task.Delay(10000, cancellationToken);
                }
            });

            var val = Task.Run(async () =>
            {
                while (cancellationToken.IsCancellationRequested == false)
                {
                    // send new random X value
                    xWriter.Reset();
                    xWriter.Write(Random.Shared.Next());

                    // send new random Y value
                    yWriter.Reset();
                    yWriter.Write(Random.Shared.Next());

                    // wait a second before updating value
                    await Task.Delay(1000, cancellationToken);
                }
            });

            var ret = Task.Run(async () =>
            {
                await foreach (var i in opResultChan.OpenRead<int>(cancellationToken))
                    System.Console.WriteLine(i);
            });

            await Task.WhenAll(fun, val, ret);
        }

    }

}