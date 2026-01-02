using Gib.Base.Collections;
using Gib.Core.Elements;

namespace Gib.Base.IO
{

    public interface ICopyTree : IElementProxy
    {

        IValueSource<DirectoryPath> Destination { get; set; }

        IStreamBinding<SetEvent<RelativeFile>> SourceFiles { get; set; }

        IStreamBinding<SetEvent<RelativeFile>> CopiedFiles { get; }

    }

}
