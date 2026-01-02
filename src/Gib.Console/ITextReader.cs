using Gib.Core.Elements;

namespace Gib.Console
{
    interface ITextReader : IElementProxy
    {

        IValueBinding<byte[]> Bytes { get; set; }

        IValueBinding<string> Chars { get; set; }

    }

}
