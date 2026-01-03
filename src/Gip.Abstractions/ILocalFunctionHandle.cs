using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Gip.Abstractions
{

    public interface ILocalFunctionHandle : IFunctionHandle
    {

        /// <summary>
        /// Gets the ID of the function.
        /// </summary>
        Guid Id { get; }

        /// <summary>
        /// Initiates a call to the function. The task returns once the call has been initiated.
        /// </summary>
        /// <param name="sources"></param>
        /// <param name="outputs"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        new ValueTask<ILocalCallHandle> CallAsync(ImmutableArray<IChannelHandle?> sources, ImmutableArray<IChannelHandle?> outputs, CancellationToken cancellationToken);

    }

}
