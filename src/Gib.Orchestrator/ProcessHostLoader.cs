using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Gib.Core.Elements;

namespace Gib.Orchestrator
{

    /// <summary>
    /// Maintains instances of <see cref="ProcessHost"/> and their association to a <see cref="HostElement" />
    /// </summary>
    class ProcessHostLoader
    {

        readonly IElementContext _context;
        readonly ProcessHostManager _manager;
        readonly ConditionalWeakTable<ProcessHost, IHostProxy> _elements = new ConditionalWeakTable<ProcessHost, IHostProxy>();

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        public ProcessHostLoader(IElementContext context)
        {
            _context = context;
            _manager = new ProcessHostManager();
        }

        /// <summary>
        /// Gets or loads the host at the specified path.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<IHostProxy?> GetOrLoadAsync(string path, CancellationToken cancellationToken)
        {
            var host = await _manager.GetOrLoadAsync(path, cancellationToken);
            if (host == null)
                return null;

            lock (_elements)
                return _elements.GetOrAdd(host, h => _context.CreateElement(ctx => new HostElement(ctx, h)));
        }

    }

}
