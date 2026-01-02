using System.Threading;
using System.Threading.Tasks;

using Gib.Core.Elements;

namespace Gib.Orchestrator
{

    /// <summary>
    /// A host element loads a specific host from a specific local directory and exposes the type reference to obtain an instance of the host.
    /// </summary>
    [Element]
    class HostLoaderElement : ElementBase, IElementWithProxy<IHostLoader>
    {

        readonly ProcessHostLoader _loader;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="loader"></param>
        public HostLoaderElement(IElementContext context) :
            base(context)
        {
            _loader = new ProcessHostLoader(context);
        }

        public ElementTypeReference? HostLoaderItemRefType { get; set; }

        /// <inheritdoc />
        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            HostLoaderItemRefType = new ElementTypeReference(RegisterElementType("ref", ctx => new HostLoaderItemRefElement(ctx, _loader) { HostPath = ctx.GetPropertyValue("hostPath") }));
        }

    }

}
