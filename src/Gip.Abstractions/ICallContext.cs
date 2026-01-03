using System;
using System.Collections.Immutable;

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
        IPipelineContext Pipeline { get; }

        /// <summary>
        /// Gets a service provider that is scoped to the call.
        /// </summary>
        IServiceProvider Services { get; }

        /// <summary>
        /// Gets the bindings of the source channels of the call.
        /// </summary>
        ImmutableArray<IChannelHandle> Sources { get; }

        /// <summary>
        /// Gets the bindings of the output channels of the call.
        /// </summary>
        ImmutableArray<IChannelHandle> Outputs { get; }

    }

}
