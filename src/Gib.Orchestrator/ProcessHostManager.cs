using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Gib.Orchestrator
{

    /// <summary>
    /// Maintains a list of running host processes, and exposes them through the <see cref="ProcessHost"/> type.
    /// </summary>
    class ProcessHostManager
    {

        readonly ConcurrentDictionary<string, Task<ProcessHost>> _hosts = new();

        /// <summary>
        /// Gets or loads the host at the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<ProcessHost> GetOrLoadAsync(string path, CancellationToken cancellationToken)
        {
            return _hosts.GetOrAdd(path, p => ProcessHost.StartAsync(p, cancellationToken));
        }

    }

}