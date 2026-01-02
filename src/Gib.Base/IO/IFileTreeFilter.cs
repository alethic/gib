using Gib.Base.Collections;
using Gib.Core.Elements;

namespace Gib.Base.IO
{

    public interface IFileTreeFilter : IElementProxy
    {

        /// <summary>
        /// Files to be filtered.
        /// </summary>
        public IStreamBinding<SetEvent<RelativeFile>> SourceFiles { get; set; }

        /// <summary>
        /// Result of applying filter.
        /// </summary>
        public IStreamBinding<SetEvent<RelativeFile>> OutputFiles { get; set; }

        /// <summary>
        /// Glob to apply to the files.
        /// </summary>
        public IValueSource<string> Glob { get; set; }

    }

}
