using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Gip.Abstractions
{

    /// <summary>
    /// Holds a reference to a local or remote function.
    /// </summary>
    public interface IFunctionHandle
    {

        /// <summary>
        /// Gets the ID of the function.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Gets the schema of the function.
        /// </summary>
        FunctionSchema Schema { get; }

        /// <summary>
        /// Initiates a call to the function. The task returns once the call has been initiated.
        /// </summary>
        /// <param name="sources"></param>
        /// <param name="services"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        ValueTask<ICallHandle> CallAsync(IServiceProvider services, ImmutableArray<SourceParameter> sources, CancellationToken cancellationToken);

    }

}
