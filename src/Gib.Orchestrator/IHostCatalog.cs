using Gib.Core.Elements;

namespace Gib.Orchestrator
{

    interface IHostCatalog : IElementProxy
    {

        IValueBinding<ElementTypeReference> RefType { get; }

    }

}
