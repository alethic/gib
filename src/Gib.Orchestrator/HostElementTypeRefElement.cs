using System.Threading;
using System.Threading.Tasks;

using Gib.Core;
using Gib.Core.Elements;

namespace Gib.Orchestrator
{

    /// <summary>
    /// This element tracks a request for an element from a host by type name.
    /// </summary>
    [Element]
    class HostElementTypeRefElement : ElementBase, IElementWithProxy<IHostElementTypeRef>
    {

        readonly ProcessHostManager _hosts;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="hosts"></param>
        public HostElementTypeRefElement(IElementContext context, ProcessHostManager hosts) :
            base(context)
        {
            _hosts = hosts;
        }

        /// <summary>
        /// Reference to the host from which to request the given element type.
        /// </summary>
        [Property("host")]
        public required ElementReference Host { get; set; }

        /// <summary>
        /// Name of the host to look up from the catalog.
        /// </summary>
        [Property("elementTypeName")]
        public required string ElementTypeName { get; set; }

        /// <summary>
        /// Reference to the host for the specified name.
        /// </summary>
        [Property("elementType")]
        public ElementTypeReference? ElementType { get; set; }

        /// <inheritdoc />
        public override Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var host = GetElement<IHostProxy>(Host.Uri);
            host.
        }

    }

}
