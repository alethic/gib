using Gib.Core.Elements;

namespace Gib.Console
{
    interface ICsPipeProj : IElementProxy
    {

        IValueBinding<string> Code { get; set; }

        IValueBinding<string> WorkingDirectory { get; set; }

        IValueBinding<ElementReference> Project { get; }

    }

}
