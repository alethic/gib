using Gib.Core.Elements;

namespace Gib.Console
{

    interface INuGetLoader : IElementProxy
    {

        IValueBinding<string> PackageId { get; set; }

        IValueBinding<string> Version { get; set; }

        IValueBinding<ElementReference> ElementReference { get; }

    }

}
