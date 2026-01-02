using System.Threading;
using System.Threading.Tasks;

using Gib.Base.Collections;
using Gib.Core;
using Gib.Core.Elements;

namespace Gib.Base.IO
{

    [Element]
    public class SyncDirectory : ElementBase
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        public SyncDirectory(IElementContext context) :
            base(context)
        {

        }

        /// <summary>
        /// Directory to watch.
        /// </summary>
        [Property("source")]
        public required DirectoryPath SourceDirectory { get; set; }

        /// <summary>
        /// Files in the source directory.
        /// </summary>
        [Property("sourceFiles")]
        public IStreamProducer<SetEvent<RelativeFile>>? SourceFiles { get; set; }

        /// <summary>
        /// Directory to watch.
        /// </summary>
        [Property("output")]
        public required DirectoryPath OutputDirectory { get; set; }

        /// <summary>
        /// Files in the target directory.
        /// </summary>
        [Property("outputFiles")]
        public IStreamProducer<SetEvent<RelativeFile>>? OutputFiles { get; set; }

        /// <summary>
        /// Only synchronizes the files that match this glob.
        /// </summary>
        [Property("filterGlob")]
        public string? FilterGlob { get; set; }

        /// <inheritdoc />
        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            /**
             * 
             * The idea here is CreateElement needs a client-side interface type. But that can be provided in the local project with a PackageReference.
             * In a DSL based language, the NuGet package can be downloaded automatically and a client side proxy can be generated.
             * 
             */

            var fileTree = CreateElement<IFileTree>();
            fileTree.Directory = ValueOf(SourceDirectory);

            // optionally apply a filter
            var files = fileTree.Files;
            if (FilterGlob is not null)
            {
                var filter = CreateElement<IFileTreeFilter>();
                filter.SourceFiles = files;
                files = filter.OutputFiles;
            }

            var copyTree = CreateElement<ICopyTree>();
            copyTree.SourceFiles = files;
            copyTree.Destination = ValueOf(OutputDirectory);

            if (SourceFiles is not null)
                await SourceFiles.BindAsync(copyTree.SourceFiles);

            if (OutputFiles is not null)
                await OutputFiles.BindAsync(copyTree.CopiedFiles);
        }

    }

}
