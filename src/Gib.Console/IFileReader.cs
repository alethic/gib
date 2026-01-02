using Gib.Base.IO;
using Gib.Core.Elements;

namespace Gib.Console
{
    interface IFileReader : IElementProxy
    {

        IValueBinding<FilePath> Source { get; set; }

        IValueBinding<byte[]> Output { get; }

    }

}
