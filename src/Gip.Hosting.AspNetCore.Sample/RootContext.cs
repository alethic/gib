using System;
using System.Threading;
using System.Threading.Tasks;

using Gip.Abstractions;
using Gip.Core;

using Microsoft.Extensions.DependencyInjection;

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
                new ChannelSchema(typeof(IFunctionHandle)),
            ]
        );

        /// <summary>
        /// Handles an individual call.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task CallAsync(ICallContext context, CancellationToken cancellationToken)
        {
            var arg0 = context.Sources[0].OpenAsync<string>(cancellationToken);
            using var ret = await context.Outputs[0].OpenAsync<IFunctionHandle>(cancellationToken);

            IFunctionHandle? childFunc = null;
            
            async ValueTask Eval(string v0, CancellationToken cancellationToken)
            {
                switch (v0)
                {
                    case "1":
                        childFunc = context.Host.RegisterFunction(new Test1Context());
                        ret.Write(childFunc);
                        break;
                    case "2":
                        childFunc = context.Host.RegisterFunction(new Test1Context());
                        ret.Write(childFunc);
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