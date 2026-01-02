using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Gip.Abstractions;

namespace Gip.Core
{

    /// <summary>
    /// Base <see cref="IFunctionContext"/> implementation that users can use to establish a new function.
    /// </summary>
    public abstract class FunctionContextBase : IFunctionContext
    {

        /// <summary>
        /// Gets the schema for the function.
        /// </summary>
        public abstract FunctionSchema Schema { get; }

        /// <summary>
        /// Handles an individual call.
        /// </summary>
        /// <param name="call"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract Task CallAsync(ICallContext call, CancellationToken cancellationToken);

        /// <summary>
        /// Invokes the specified action for each element of the enumerable.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="async"></param>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async Task ForEachAsync<T>(IAsyncEnumerable<T> async, Func<T, CancellationToken, ValueTask> action, CancellationToken cancellationToken)
        {
            await foreach (var i in async)
                await action(i, cancellationToken);
        }

    }

}
