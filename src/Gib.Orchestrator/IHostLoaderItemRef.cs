using Gib.Core.Elements;

namespace Gib.Orchestrator
{

    public interface IHostLoaderItemRef : IElementProxy
    {

        public IValueBinding<string> Path { get; set; }

        public IValueBinding<ElementReference> Host { get; }

    }

}
