using System.Threading;
using System.Threading.Tasks;

namespace Gip.Abstractions
{

    /// <summary>
    /// User-implementable interface to develop a callable function..
    /// </summary>
    public interface IFunctionContext
    {

        /// <summary>
        /// Gets the schema for the function.
        /// </summary>
        FunctionSchema Schema { get; }

        /// <summary>
        /// Implementation of a functions processing. Accepts an <see cref="ICallContext"/> that contains the inputs and outputs.
        /// Execution is considered complete when this method exits.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task CallAsync(ICallContext context, CancellationToken cancellationToken);

    }

}