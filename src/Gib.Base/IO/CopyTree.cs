using System.Collections.Immutable;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Gib.Base.Collections;
using Gib.Core;
using Gib.Core.Elements;

namespace Gib.Base.IO
{

    [Element]
    public class CopyTree : ElementBase, IElementWithProxy<ICopyTree>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        public CopyTree(IElementContext context) :
            base(context)
        {

        }

        /// <summary>
        /// Directory to watch.
        /// </summary>
        [Property("destination")]
        public required DirectoryPath Destination { get; set; }

        /// <summary>
        /// Set of files to be copied.
        /// </summary>
        [Property("sourceFiles")]
        public required IStreamConsumer<SetEvent<RelativeFile>> SourceFiles { get; set; }

        /// <summary>
        /// Set of files that have been copied.
        /// </summary>
        [Property("copiedFiles")]
        public IStreamProducer<SetEvent<RelativeFile>>? CopiedFiles { get; set; }

        /// <inheritdoc />
        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (CopiedFiles is not null)
            {
                // we are replaying all of the files
                await CopiedFiles.ResetAsync();

                // listen to source file events
                await SourceFiles.Stream(async @event =>
                {
                    switch (@event)
                    {
                        case SetAddEvent<RelativeFile> addEvent:
                            {
                                // copy file to new path
                                var newPath = Path.Combine(Destination.AbsolutePath, addEvent.Item.RelativePath);
                                File.Copy(addEvent.Item.AbsolutePath, newPath);

                                // add new copied item
                                await CopiedFiles.SendAsync(new SetAddEvent<RelativeFile>(new RelativeFile(newPath, addEvent.Item.RelativePath)));

                                break;
                            }
                        case SetAddManyEvent<RelativeFile> addManyEvent:
                            {
                                var copiedFiles = ImmutableHashSet.CreateBuilder<RelativeFile>();

                                // copy file to new path
                                foreach (var file in addManyEvent.Items)
                                {
                                    var newPath = Path.Combine(Destination.AbsolutePath, file.RelativePath);
                                    File.Copy(file.AbsolutePath, newPath);
                                    copiedFiles.Add(new RelativeFile(newPath, file.RelativePath));
                                }

                                await CopiedFiles.SendAsync(new SetAddManyEvent<RelativeFile>(copiedFiles.ToImmutable()));

                                break;
                            }
                        case SetRemoveEvent<RelativeFile> removeEvent:
                            {
                                var oldPath = Path.Combine(Destination.AbsolutePath, removeEvent.Item.RelativePath);
                                if (File.Exists(oldPath))
                                    File.Delete(oldPath);

                                await CopiedFiles.SendAsync(new SetRemoveEvent<RelativeFile>(new RelativeFile(oldPath, removeEvent.Item.RelativePath)));

                                break;
                            }
                        case SetRemoveManyEvent<RelativeFile> removeManyEvent:
                            {
                                var removedFiles = ImmutableHashSet.CreateBuilder<RelativeFile>();

                                foreach (var file in removeManyEvent.Items)
                                {
                                    var oldPath = Path.Combine(Destination.AbsolutePath, file.RelativePath);
                                    if (File.Exists(oldPath))
                                        File.Delete(oldPath);

                                    removedFiles.Add(new RelativeFile(oldPath, file.RelativePath));
                                }

                                await CopiedFiles.SendAsync(new SetRemoveManyEvent<RelativeFile>(removedFiles.ToImmutable()));

                                break;
                            }
                    }
                }, cancellationToken);
            }
            else
            {
                // listen to source file events
                await SourceFiles.Stream(async @event =>
                {
                    switch (@event)
                    {
                        case SetAddEvent<RelativeFile> addEvent:
                            {
                                // copy file to new path
                                var newPath = Path.Combine(Destination.AbsolutePath, addEvent.Item.RelativePath);
                                File.Copy(addEvent.Item.AbsolutePath, newPath);
                                break;
                            }
                        case SetAddManyEvent<RelativeFile> addManyEvent:
                            {
                                // copy file to new path
                                foreach (var file in addManyEvent.Items)
                                {
                                    var newPath = Path.Combine(Destination.AbsolutePath, file.RelativePath);
                                    File.Copy(file.AbsolutePath, newPath);
                                }

                                break;
                            }
                        case SetRemoveEvent<RelativeFile> removeEvent:
                            {
                                var oldPath = Path.Combine(Destination.AbsolutePath, removeEvent.Item.RelativePath);
                                if (File.Exists(oldPath))
                                    File.Delete(oldPath);

                                break;
                            }
                        case SetRemoveManyEvent<RelativeFile> removeManyEvent:
                            {
                                foreach (var file in removeManyEvent.Items)
                                {
                                    var oldPath = Path.Combine(Destination.AbsolutePath, file.RelativePath);
                                    if (File.Exists(oldPath))
                                        File.Delete(oldPath);
                                }

                                break;
                            }
                    }
                }, cancellationToken);
            }
        }

    }

}
