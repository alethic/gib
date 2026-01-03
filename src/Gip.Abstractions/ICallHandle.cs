using System;
using System.Collections.Immutable;

namespace Gip.Abstractions
{

    public interface ICallHandle : IDisposable, IAsyncDisposable
    {

        /// <summary>
        /// Gets the output parameters of the call.
        /// </summary>
        ImmutableArray<IWritableChannelHandle> Outputs { get; }

    }

}
