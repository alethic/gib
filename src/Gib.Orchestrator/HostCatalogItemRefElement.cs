using System.Threading;
using System.Threading.Tasks;

using Gib.Core;
using Gib.Core.Elements;

namespace Gib.Orchestrator
{

    [Element]
    class HostCatalogItemRefElement : ElementBase
    {

        readonly HostCatalog _catalog;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="catalog"></param>
        public HostCatalogItemRefElement(IElementContext context, HostCatalog catalog) :
            base(context)
        {
            _catalog = catalog;
        }

        /// <summary>
        /// Name of the host to look up from the catalog.
        /// </summary>
        [Property("hostName")]
        public required string HostName { get; set; }

        /// <summary>
        /// Reference to the host for the specified name.
        /// </summary>
        [Property("host")]
        public required IValueProducer<ElementReference> Host { get; set; }

        /// <inheritdoc />
        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var root = GetElement<IRoot>(Root);

            if (HostName == "dotnet")
            {
                Host.Bind(root.DotNetHost);
            }
            else
            {
                var hostInfo = await _catalog.FindHostAsync(HostName);
                if (hostInfo is not null)
                {
                    var hostLoaderItemRefType = await root.HostLoaderItemRefType.GetValueAsync(cancellationToken);
                    var hostLoaderItemRef = CreateElement<IHostLoaderItemRef>(hostLoaderItemRefType.Uri);
                    hostLoaderItemRef.Path = ValueOf(hostInfo.Path);
                    Host.Bind(hostLoaderItemRef.Host);
                }
            }
        }

    }

}