using Gib.Core.Elements;

namespace Gib.Orchestrator
{

    public interface IRoot : IElementProxy
    {

        IValueBinding<ElementReference> DotNetHost { get; }

        IValueBinding<ElementReference> HostLoader { get; }
        
        IValueBinding<ElementTypeReference> HostLoaderItemRefType { get; }

        IValueBinding<ElementReference> HostCatalog { get; }

        IValueBinding<ElementTypeReference> HostCatalogItemRefType { get; }

    }

}
