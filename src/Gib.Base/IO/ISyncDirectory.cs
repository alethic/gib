using Gib.Base.Collections;
using Gib.Core.Elements;

namespace Gib.Base.IO
{

    public interface ISyncDirectory
    {

        IValueSource<DirectoryPath> SourceDirectory { get; set; }

        IStreamBinding<SetEvent<RelativeFile>> SourceFiles { get; set; }

        IValueSource<DirectoryPath> OutputDirectory { get; set; }

        IStreamBinding<SetEvent<RelativeFile>> OutputFiles { get; }

    }

}
