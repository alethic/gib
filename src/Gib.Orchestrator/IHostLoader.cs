using Gib.Core.Elements;

namespace Gib.Orchestrator
{

    interface IHostLoader : IElementProxy
    {

        IValueBinding<ElementTypeReference> RefType { get; }

    }

}