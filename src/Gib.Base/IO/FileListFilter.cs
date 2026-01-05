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
    /// This elements reads from a set of absolute files and filters the files based on a glob.
    /// </summary>
    public class FileListFilter : FunctionContextBase
    {

        /// <summary>
        /// Gets the schema for the function.
        /// </summary>
        public override FunctionSchema Schema { get; } = FunctionSchema.CreateBuilder()
            .Source<SetSignal<AbsoluteFile>>()
            .Source<ValueSignal<string>>()
            .Output<SetSignal<AbsoluteFile>>()
            .Build();

        /// <summary>
        /// Handles an individual call.
        /// </summary>
        /// <param name="call"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task CallAsync(ICallContext call, CancellationToken cancellationToken)
        {
            using var outputs = call.Outputs[0].EmitSet<AbsoluteFile>();

            // each time the glob changes reevaluate
            await foreach (var glob in call.Sources[1].CollectValue<string>(cancellationToken))
            {
                var matcher = new Matcher();
                matcher.AddInclude(glob);

                // proxy the set signals across, filtering items out
                await foreach (var signal in call.Sources[0].OpenRead<SetSignal<AbsoluteFile>>(cancellationToken))
                {
                    switch (signal)
                    {
                        case SetAddSignal<AbsoluteFile> addSignal:
                            {
                                var rootDir = Path.GetDirectoryName(addSignal.Item.AbsolutePath);
                                if (matcher.Match(rootDir!, Path.GetFileName(addSignal.Item.AbsolutePath)).HasMatches)
                                    outputs.Add(addSignal.Item);

                                break;
                            }
                        case SetAddManySignal<AbsoluteFile> addManySignal:
                            {
                                var filteredFiles = ImmutableArray.CreateBuilder<AbsoluteFile>();

                                foreach (var file in addManySignal.Items)
                                {
                                    var rootDir = Path.GetDirectoryName(file.AbsolutePath);
                                    if (matcher.Match(rootDir!, Path.GetFileName(file.AbsolutePath)).HasMatches)
                                        filteredFiles.Add(file);
                                }

                                if (filteredFiles.Count == 1)
                                    outputs.Add(filteredFiles[0]);
                                else if (filteredFiles.Count >= 2)
                                    outputs.AddRange(filteredFiles.DrainToImmutable());

                                break;
                            }
                        case SetRemoveSignal<AbsoluteFile> removeSignal:
                            {
                                var rootDir = Path.GetDirectoryName(removeSignal.Item.AbsolutePath);
                                if (matcher.Match(rootDir!, Path.GetFileName(removeSignal.Item.AbsolutePath)).HasMatches)
                                    outputs.Remove(removeSignal.Item);

                                break;
                            }
                        case SetRemoveManySignal<AbsoluteFile> removeManyEvent:
                            {
                                var filteredFiles = ImmutableArray.CreateBuilder<AbsoluteFile>();

                                foreach (var file in removeManyEvent.Items)
                                {
                                    var rootDir = Path.GetDirectoryName(file.AbsolutePath);
                                    if (matcher.Match(rootDir!, Path.GetFileName(file.AbsolutePath)).HasMatches)
                                        filteredFiles.Add(file);
                                }

                                if (filteredFiles.Count == 1)
                                    outputs.Remove(filteredFiles[0]);
                                else if (filteredFiles.Count >= 2)
                                    outputs.RemoveRange(filteredFiles.DrainToImmutable());

                                break;
                            }
                        case SetClearSignal<AbsoluteFile>:
                            outputs.Clear();
                            break;
                        case SetFreezeSignal<AbsoluteFile>:
                            outputs.Freeze();
                            break;
                        case SetResumeSignal<AbsoluteFile>:
                            outputs.Resume();
                            break;
                    }
                }
            }
        }

    }

}