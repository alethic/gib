using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Gib.Core;

using Gip.Abstractions;
using Gip.Base;
using Gip.Base.Collections;
using Gip.Core;

namespace Gib.Base.IO
{

    [Element]
    public class CopyTree : FunctionContextBase
    {

        /// <summary>
        /// Gets the schema for the function.
        /// </summary>
        public override FunctionSchema Schema { get; } = FunctionSchema.CreateBuilder()
            .Source<ValueSignal<AbsoluteFile>>()
            .Source<SetSignal<RelativeFile>>()
            .Output<SetSignal<RelativeFile>>()
            .Output<SequenceSignal<string>>()
            .Build();

        /// <summary>
        /// Handles an individual call.
        /// </summary>
        /// <param name="call"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task CallAsync(ICallContext call, CancellationToken cancellationToken)
        {
            var copiedFiles = call.Outputs[0].EmitSet<RelativeFile>();

            // maintain state about files we have copied
            var copiedFilesSet = new Dictionary<string, RelativeFile>();

            await foreach (var destination in call.Sources[0].CollectValue<AbsoluteFile>(cancellationToken))
            {
                // delete all files recorded in copied files set initially
                {
                    // remove all recorded files
                    foreach (var removedFile in copiedFilesSet.Values)
                        if (File.Exists(removedFile.AbsolutePath))
                            File.Delete(removedFile.AbsolutePath);

                    // clear our record
                    copiedFilesSet.Clear();

                    // send notice of files cleared
                    copiedFiles.Clear();
                }

                // read from the feed of source files
                await foreach (var signal in call.Sources[1].OpenRead<SetSignal<RelativeFile>>(cancellationToken))
                {
                    switch (signal)
                    {
                        case SetAddSignal<RelativeFile> addSignal:
                            {
                                // copy file to new path
                                var newPath = Path.Combine(destination.AbsolutePath, addSignal.Item.RelativePath);
                                File.Copy(addSignal.Item.AbsolutePath, newPath);

                                // record new copied item
                                var newFile = RelativeFile.FromPath(newPath, addSignal.Item.RelativePath);
                                copiedFilesSet.Add(newFile.AbsolutePath, newFile);

                                // send notice of new file
                                copiedFiles.Add(newFile);

                                break;
                            }
                        case SetAddManySignal<RelativeFile> addManySignal:
                            {
                                var newFiles = ImmutableArray.CreateBuilder<RelativeFile>();

                                // copy file to new path
                                foreach (var addedFile in addManySignal.Items)
                                {
                                    // copy file to new path
                                    var newPath = Path.Combine(destination.AbsolutePath, addedFile.RelativePath);
                                    var newFile = RelativeFile.FromPath(newPath, addedFile.RelativePath);
                                    File.Copy(addedFile.AbsolutePath, newFile.AbsolutePath);

                                    // record new copied item
                                    copiedFilesSet.Add(newFile.AbsolutePath, newFile);

                                    // send notice of new file
                                    newFiles.Add(newFile);
                                }

                                // send notice of new files
                                copiedFiles.AddRange(newFiles.MoveToImmutable());

                                break;
                            }
                        case SetRemoveSignal<RelativeFile> removeSignal:
                            {
                                var oldPath = Path.Combine(destination.AbsolutePath, removeSignal.Item.RelativePath);
                                if (copiedFilesSet.TryGetValue(oldPath, out var oldFile))
                                {
                                    if (File.Exists(oldFile.AbsolutePath))
                                        File.Delete(oldFile.AbsolutePath);

                                    // record new deleted item
                                    copiedFilesSet.Remove(oldPath);

                                    // send notice of old file
                                    copiedFiles.Remove(oldFile);
                                }

                                break;
                            }
                        case SetRemoveManySignal<RelativeFile> removeManySignal:
                            {
                                var oldFiles = ImmutableArray.CreateBuilder<RelativeFile>();

                                foreach (var removedFile in removeManySignal.Items)
                                {
                                    var oldPath = Path.Combine(destination.AbsolutePath, removedFile.RelativePath);
                                    if (copiedFilesSet.TryGetValue(oldPath, out var oldFile))
                                    {
                                        if (File.Exists(oldFile.AbsolutePath))
                                            File.Delete(oldFile.AbsolutePath);

                                        // record new deleted item
                                        copiedFilesSet.Remove(oldPath);

                                        // send notice of old file
                                        oldFiles.Add(oldFile);
                                    }
                                }

                                // send notice of old files
                                copiedFiles.RemoveRange(oldFiles.MoveToImmutable());

                                break;
                            }
                        case SetClearSignal<RelativeFile> clearSignal:
                            {
                                // remove all recorded files
                                foreach (var removedFile in copiedFilesSet.Values)
                                    if (File.Exists(removedFile.AbsolutePath))
                                        File.Delete(removedFile.AbsolutePath);

                                // clear our record
                                copiedFilesSet.Clear();

                                // send notice of files cleared
                                copiedFiles.Clear();

                                break;
                            }
                    }
                }
            }
        }

    }

}
