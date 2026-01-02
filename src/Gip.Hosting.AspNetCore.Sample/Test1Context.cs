using System.Threading;
using System.Threading.Tasks;

using Gip.Abstractions;
using Gip.Core;

namespace Gip.Hosting.AspNetCore.Sample
{

    public class Test1Context : FunctionContextBase
    {

        /// <summary>
        /// Gets the schema for the function.
        /// </summary>
        public override FunctionSchema Schema { get; } = new FunctionSchema(
            [
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
            var arg0 = call.Sources[0].OpenAsync<int>(cancellationToken);
            var arg1 = call.Sources[1].OpenAsync<int>(cancellationToken);
            using var ret = await call.Outputs[0].OpenAsync<int>(cancellationToken);
            
            int val0 = 0;
            int val1 = 0;

            async ValueTask Eval(int v0, int v1) => ret.Write(val0 + val1);
            ValueTask Eval0(int v0, CancellationToken cancellationToken) => Eval(val0 = v0, val1);
            ValueTask Eval1(int v1, CancellationToken cancellationToken) => Eval(val0, val1 = v1);

            // listen to all of our parameter subscriptions until the invocation is canceled
            await Task.WhenAll(
                ForEachAsync(arg0, Eval0, cancellationToken),
                ForEachAsync(arg1, Eval1, cancellationToken));

            // our return value is finished
            ret.Complete();
        }

    }

}