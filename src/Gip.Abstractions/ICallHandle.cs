using System;
using System.Collections.Immutable;

namespace Gip.Abstractions
{

    public interface ICallHandle : IDisposable, IAsyncDisposable
    {

        /// <summary>
        /// Gets the function that was called.
        /// </summary>
        IFunctionHandle Function { get; }

        /// <summary>
        /// Gets the output parameters of the call.
        /// </summary>
        ImmutableArray<IReadableChannelHandle> Outputs { get; }

    }

}
