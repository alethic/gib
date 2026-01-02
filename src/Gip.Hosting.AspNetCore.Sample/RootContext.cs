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
                new ChannelSchema(typeof(string)),
            ],
            [
                new ChannelSchema(typeof(FunctionReference)),
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
            var arg0 = call.Sources[0].OpenAsync<string>(cancellationToken);
            using var ret = await call.Outputs[0].OpenAsync<FunctionReference>(cancellationToken);

            IFunctionHandle? childFunc = null;
            
            async ValueTask Eval(string v0, CancellationToken cancellationToken)
            {
                switch (v0)
                {
                    case "1":
                        childFunc = call.Host.CreateFunction(new Test1Context());
                        ret.Write(call.Host.GetFunctionReference(childFunc));
                        break;
                    case "2":
                        childFunc = call.Host.CreateFunction(new Test2Context());
                        ret.Write(call.Host.GetFunctionReference(childFunc));
                        break;
                }
            }

            // listen to all of our parameter subscriptions until the invocation is canceled
            await Task.WhenAll(ForEachAsync(arg0, Eval, cancellationToken));

            // our return value is finished
            ret.Complete();
        }

    }

}