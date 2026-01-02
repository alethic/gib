using System.Threading;
using System.Threading.Tasks;

using Gib.Core;
using Gib.Core.Elements;

namespace Gib.Orchestrator
{

    [Element]
    class HostLoaderItemRefElement : ElementBase, IElementWithProxy<IHostLoaderItemRef>
    {

        readonly ProcessHostLoader _hostLoader;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="hostLoader"></param>
        public HostLoaderItemRefElement(IElementContext context, ProcessHostLoader hostLoader) :
            base(context)
        {
            _hostLoader = hostLoader;
        }

        /// <summary>
        /// Name of the host to look up from the catalog.
        /// </summary>
        [Property("hostPath")]
        public required string HostPath { get; set; }

        /// <summary>
        /// Reference to the host for the specified name.
        /// </summary>
        [Property("host")]
        public ElementReference? Host { get; set; }

        /// <inheritdoc />
        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            Host = null;

            var host = await _hostLoader.GetOrLoadAsync(HostPath, cancellationToken);
            if (host != null)
                Host = new ElementReference(host.ElementUri);
        }

    }

}
