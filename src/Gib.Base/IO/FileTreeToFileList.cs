using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Gip.Abstractions;
using Gip.Base.Collections;
using Gip.Core;

namespace Gib.Base.IO
{

    /// <summary>
    /// This elements reads from a set of relative files and filters the files based on a glob.
    /// </summary>
    public class FileTreeToFileList : FunctionContextBase
    {

        /// <summary>
        /// Gets the schema for the function.
        /// </summary>
        public override FunctionSchema Schema { get; } = FunctionSchema.CreateBuilder()
            .Source<SetSignal<RelativeFile>>()
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

            await foreach (var signal in call.Sources[0].OpenRead<SetSignal<RelativeFile>>(cancellationToken))
            {
                switch (signal)
                {
                    case SetAddSignal<RelativeFile> addSignal:
                        outputs.Add(new AbsoluteFile(addSignal.Item.AbsolutePath, addSignal.Item.Statistics));
                        break;
                    case SetAddManySignal<RelativeFile> addManySignal:
                        outputs.AddRange(addManySignal.Items.Select(i => new AbsoluteFile(i.AbsolutePath, i.Statistics)).ToImmutableArray());
                        break;
                    case SetRemoveSignal<RelativeFile> removeSignal:
                        outputs.Remove(new AbsoluteFile(removeSignal.Item.AbsolutePath, removeSignal.Item.Statistics));
                        break;
                    case SetRemoveManySignal<RelativeFile> removeManySignal:
                        outputs.RemoveRange(removeManySignal.Items.Select(i => new AbsoluteFile(i.AbsolutePath, i.Statistics)).ToImmutableArray());
                        break;
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