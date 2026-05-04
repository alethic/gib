using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
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
        /// Clears all of the files in cache.
        /// </summary>
        /// <param name="cached"></param>
        /// <param name="copied"></param>
        void Clear(Dictionary<string, RelativeFile> cached, SetEmitter<RelativeFile> copied)
        {
            // remove all recorded files
            foreach (var removedFile in cached.Values)
                if (File.Exists(removedFile.AbsolutePath))
                    File.Delete(removedFile.AbsolutePath);

            // clear our record
            cached.Clear();

            // send notice of files cleared
            copied.Clear();
        }

        /// <summary>
        /// Handles an individual call.
        /// </summary>
        /// <param name="call"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task CallAsync(ICallContext call, CancellationToken cancellationToken)
        {
            var cached = new Dictionary<string, RelativeFile>();
            var copied = call.Outputs[0].EmitSet<RelativeFile>();

            await foreach (var destination in call.Sources[0].CollectValue<AbsoluteFile>(cancellationToken))
            {
                // when the destination changes, we clear any files we have already copied
                Clear(cached, copied);

                await foreach (var reader in call.Sources[1].Reader<SetSignal<RelativeFile>>(cancellationToken))
                {
                    // when the reader changes, we clear any files we have already copied
                    Clear(cached, copied);

                    // process each signal of the new set
                    await foreach (var signal in reader)
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
                                    cached.Add(newFile.AbsolutePath, newFile);

                                    // send notice of new file
                                    copied.Add(newFile);

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
                                        cached.Add(newFile.AbsolutePath, newFile);

                                        // send notice of new file
                                        newFiles.Add(newFile);
                                    }

                                    // send notice of new files
                                    copied.AddRange(newFiles.MoveToImmutable());

                                    break;
                                }
                            case SetRemoveSignal<RelativeFile> removeSignal:
                                {
                                    var oldPath = Path.Combine(destination.AbsolutePath, removeSignal.Item.RelativePath);
                                    if (cached.TryGetValue(oldPath, out var oldFile))
                                    {
                                        if (File.Exists(oldFile.AbsolutePath))
                                            File.Delete(oldFile.AbsolutePath);

                                        // record new deleted item
                                        cached.Remove(oldPath);

                                        // send notice of old file
                                        copied.Remove(oldFile);
                                    }

                                    break;
                                }
                            case SetRemoveManySignal<RelativeFile> removeManySignal:
                                {
                                    var oldFiles = ImmutableArray.CreateBuilder<RelativeFile>();

                                    foreach (var removedFile in removeManySignal.Items)
                                    {
                                        var oldPath = Path.Combine(destination.AbsolutePath, removedFile.RelativePath);
                                        if (cached.TryGetValue(oldPath, out var oldFile))
                                        {
                                            if (File.Exists(oldFile.AbsolutePath))
                                                File.Delete(oldFile.AbsolutePath);

                                            // record new deleted item
                                            cached.Remove(oldPath);

                                            // send notice of old file
                                            oldFiles.Add(oldFile);
                                        }
                                    }

                                    // send notice of old files
                                    copied.RemoveRange(oldFiles.MoveToImmutable());

                                    break;
                                }
                            case SetClearSignal<RelativeFile> clearSignal:
                                Clear(cached, copied);
                                break;
                            default:
                                throw new InvalidOperationException();
                        }
                    }
                }
            }
        }

    }

}
