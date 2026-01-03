using System.Threading;
using System.Threading.Tasks;

using Gip.Abstractions;
using Gip.Core;

namespace Gip.Hosting.AspNetCore.Sample
{

    public class AdderContext : FunctionContextBase
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
            var arg0 = call.Sources[0].OpenRead<int>(cancellationToken);
            var arg1 = call.Sources[1].OpenRead<int>(cancellationToken);
            using var ret = call.Outputs[0].OpenWrite<int>();

            int? val0 = null;
            int? val1 = null;

            void Eval()
            {
                if (val0 is int v0 && val1 is int v1)
                {
                    ret.Reset();
                    ret.Write(v0 + v1);
                }
            }

            ValueTask Eval0(int v0, CancellationToken cancellationToken)
            {
                lock (call)
                {
                    val0 = v0;
                    Eval();
                }

                return default;
            }

            ValueTask Eval1(int v1, CancellationToken cancellationToken)
            {
                lock (call)
                {
                    val1 = v1;
                    Eval();
                }

                return default;
            }

            // listen to all of our parameter subscriptions until the invocation is canceled
            await Task.WhenAll(
                ForEachAsync(arg0, Eval0, cancellationToken),
                ForEachAsync(arg1, Eval1, cancellationToken));
        }

    }

}