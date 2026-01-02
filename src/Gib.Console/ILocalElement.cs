using Gib.Core.Elements;

namespace Gib.Console
{

    interface ILocalElement : IElementProxy
    {

        public IValueBinding<bool> DoThing { get; set; }

        public IValueBinding<bool> DidThing { get; }

    }

}
