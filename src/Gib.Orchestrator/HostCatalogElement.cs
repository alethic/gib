using System.Threading;
using System.Threading.Tasks;

using Gib.Core.Elements;

namespace Gib.Orchestrator
{

    /// <summary>
    /// Maintains a catalog of the available host types.
    /// </summary>
    [Element]
    class HostCatalogElement : ElementBase, IElementWithProxy<IHostCatalog>
    {

        readonly HostCatalog _catalog;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        public HostCatalogElement(IElementContext context, HostCatalog catalog) :
            base(context)
        {
            _catalog = catalog;
        }

        public ElementTypeReference? HostCatalogItemRefType { get; set; }

        /// <inheritdoc />
        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            HostCatalogItemRefType = new ElementTypeReference(RegisterElementType<HostCatalogItemRefElement>([_catalog]));
        }

    }

}
