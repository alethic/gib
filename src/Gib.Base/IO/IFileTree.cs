using Gib.Base.Collections;
using Gib.Core.Elements;

namespace Gib.Base.IO
{

    public interface IFileTree : IElementProxy
    {

        IValueSource<DirectoryPath> Directory { get; set; }

        IStreamBinding<SetEvent<RelativeFile>> Files { get; }

    }

}
