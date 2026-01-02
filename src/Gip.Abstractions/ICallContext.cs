using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading;

namespace Gip.Abstractions
{

    /// <summary>
    /// Encapsulates information regarding a specific service call.
    /// </summary>
    public interface ICallContext
    {

        /// <summary>
        /// Gets a reference to the pipeline host.
        /// </summary>
        IPipelineHost Host { get; }

        /// <summary>
        /// Gets a service provider that is scoped to the call.
        /// </summary>
        IServiceProvider Services { get; }

        /// <summary>
        /// Gets the bindings of the source channels of the call.
        /// </summary>
        ImmutableArray<SourceBinding> Sources { get; }

        /// <summary>
        /// Gets the bindings of the output channels of the call.
        /// </summary>
        ImmutableArray<OutputBinding> Outputs { get; }

        /// <summary>
        /// Opens a remote channel for reading.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        IAsyncEnumerable<T> OpenRemoteAsync<T>(Uri channelUri, CancellationToken cancellationToken);

    }

}
