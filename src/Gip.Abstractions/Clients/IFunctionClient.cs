using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

namespace Gip.Abstractions.Clients
{

    /// <summary>
    /// A <see cref="IFunctionClient"/> represents a connection to a remote function.
    /// </summary>
    public interface IFunctionClient : IDisposable, IAsyncDisposable
    {

        /// <summary>
        /// Calls the function, returning a <see cref="ICallClient"/>.
        /// </summary>
        /// <param name="sources"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        ValueTask<ICallClient> CallAsync(ImmutableArray<SourceParameter> sources, CancellationToken cancellationToken);

    }

}
