using System;
using System.Threading;

namespace Gip.Core
{

    /// <summary>
    /// Holds a lock on a particular <see cref="GipObject"/>. Provides a synchronization context using the `using` syntax.
    /// </summary>
    public readonly struct GipLock : IDisposable
    {

        readonly GipObject sync;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="sync"></param>
        public GipLock(GipObject sync)
        {
            this.sync = sync ?? throw new ArgumentNullException(nameof(sync));
            Monitor.Enter(this.sync);
        }

        /// <summary>
        /// Disposes of the instance, releasing the lock.
        /// </summary>
        public void Dispose()
        {
            Monitor.Exit(this.sync);
        }

    }

}
