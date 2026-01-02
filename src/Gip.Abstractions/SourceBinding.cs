using System.Collections.Generic;
using System.Threading;

namespace Gip.Abstractions
{

    /// <summary>
    /// Describes a channel made available as a source for a function call.
    /// </summary>
    public abstract class SourceBinding : Binding
    {

        /// <summary>
        /// Opens a channel to read from the specified <see cref="SourceBinding"/>.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract IAsyncEnumerable<T> OpenAsync<T>(CancellationToken cancellationToken);

    }

}
