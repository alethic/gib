using System;
using System.Collections.Immutable;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Gip.Abstractions;
using Gip.Base;
using Gip.Base.Collections;
using Gip.Core;

using Microsoft.Extensions.FileSystemGlobbing;

namespace Gib.Base.IO
{

    /// <summary>
    /// This elements reads from a set of relative files and filters the files based on a glob.
    /// </summary>
    public class FileTreeFilter : FunctionContextBase
    {

        /// <summary>
        /// Gets the schema for the function.
        /// </summary>
        public override FunctionSchema Schema { get; } = FunctionSchema.CreateBuilder()
            .Source<SetSignal<RelativeFile>>()
            .Source<ValueSignal<string>>()
            .Output<SetSignal<RelativeFile>>()
            .Build();

        /// <summary>
        /// Handles an individual call.
        /// </summary>
        /// <param name="call"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task CallAsync(ICallContext call, CancellationToken cancellationToken)
        {
            using var outputs = call.Outputs[0].EmitSet<RelativeFile>();

            await foreach (var glob in call.Sources[1].CollectValue<string>(cancellationToken))
            {
                var matcher = new Matcher();
                matcher.AddInclude(glob);

                await foreach (var signal in call.Sources[0].OpenRead<SetSignal<RelativeFile>>(cancellationToken))
                {
                    switch (signal)
                    {
                        case SetAddSignal<RelativeFile> addEvent:
                            {
                                if (matcher.Match(addEvent.Item.AbsolutePath, Path.Combine(addEvent.Item.AbsolutePath, addEvent.Item.RelativePath)).HasMatches)
                                    outputs.Add(addEvent.Item);

                                break;
                            }
                        case SetAddManySignal<RelativeFile> addManyEvent:
                            {
                                var filteredFiles = ImmutableHashSet.CreateBuilder<RelativeFile>();

                                foreach (var file in addManyEvent.Items)
                                    if (matcher.Match(file.AbsolutePath, Path.Combine(file.AbsolutePath, file.RelativePath)).HasMatches)
                                        filteredFiles.Add(file);

                                outputs.AddRange(filteredFiles.ToImmutable());

                                break;
                            }
                        case SetRemoveSignal<RelativeFile> removeEvent:
                            {
                                if (matcher.Match(removeEvent.Item.AbsolutePath, Path.Combine(removeEvent.Item.AbsolutePath, removeEvent.Item.RelativePath)).HasMatches)
                                    outputs.Remove(removeEvent.Item);

                                break;
                            }
                        case SetRemoveManySignal<RelativeFile> removeManyEvent:
                            {
                                var filteredFiles = ImmutableHashSet.CreateBuilder<RelativeFile>();

                                foreach (var file in removeManyEvent.Items)
                                    if (matcher.Match(file.AbsolutePath, Path.Combine(file.AbsolutePath, file.RelativePath)).HasMatches)
                                        filteredFiles.Add(file);

                                outputs.RemoveRange(filteredFiles.ToImmutable());

                                break;
                            }
                        case SetClearSignal<RelativeFile>:
                            outputs.Clear();
                            break;
                        case SetFreezeSignal<RelativeFile>:
                            outputs.Freeze();
                            break;
                        case SetResumeSignal<RelativeFile>:
                            outputs.Resume();
                            break;
                    }
                }
            }
        }

    }

}