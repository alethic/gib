using System.Threading;
using System.Threading.Tasks;

using Gib.Core;
using Gib.Core.Elements;

namespace Gib.Orchestrator
{

    /// <summary>
    /// A single global instance of the root element is registered by the orchestrator.
    /// A reference to this instance is then passed down into whatever specific element the orchestrator starts with.
    /// </summary>
    [Element]
    public class RootElement : IElement, IElementWithProxy<IRoot>
    {

        readonly IElementContext _context;
        private readonly HostCatalog _catalog;
        readonly DotNetHost _dotNetHost;

        IHostProxy? _dotnetHostElement;
        IHostLoader? _hostLoaderElement;
        IHostCatalog? _hostCatalogElement;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="catalog"></param>
        public RootElement(IElementContext context, HostCatalog catalog)
        {
            _context = context;
            _catalog = catalog;
            _dotNetHost = new DotNetHost();
        }

        /// <summary>
        /// Internal dotnet host element.
        /// </summary>
        [Property("dotnet")]
        public ElementReference DotNetHost { get; set; }

        /// <summary>
        /// Exposed host loader element.
        /// </summary>
        [Property("hostLoader")]
        public ElementReference HostLoader { get; set; }

        /// <summary>
        /// Exposed host item reference type.
        /// </summary>
        [Property("hostLoaderItemRefType")]
        public required IValueProducer<ElementTypeReference> HostLoaderItemRefType { get; set; }

        /// <summary>
        /// Exposed host catalog element.
        /// </summary>
        [Property("hostCatalog")]
        public ElementReference HostCatalog { get; set; }

        /// <summary>
        /// Exposed host catalog element.
        /// </summary>
        [Property("hostCatalogItemRefType")]
        public required IValueProducer<ElementTypeReference> HostCatalogItemRefType { get; set; }

        /// <inheritdoc />
        public Task ExecuteAsync(CancellationToken cancellationToken)
        {
            _dotnetHostElement ??= _context.CreateElement<IHostProxy, HostElement>([_dotNetHost]);
            DotNetHost = new ElementReference(_dotnetHostElement.ElementUri);

            // register host loader element and expose loader item ref type
            _hostLoaderElement ??= _context.CreateElement<IHostLoader, HostLoaderElement>();
            HostLoader = new ElementReference(_hostLoaderElement.ElementUri);
            HostLoaderItemRefType.BindAsync(_hostLoaderElement.RefType);

            // register host catalog element and expose catalog item ref type
            _hostCatalogElement ??= _context.CreateElement<IHostCatalog, HostCatalogElement>([]);
            HostCatalog = new ElementReference(_hostCatalogElement.ElementUri);
            HostCatalogItemRefType.BindAsync(_hostCatalogElement.RefType);

            _hostElementTypeRefElement ??= _context.RegisterElementType<IHostElementTypeRef, HostElementTypeRefElement([]);


            return Task.CompletedTask;
        }

    }

}
