using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Gip.Abstractions;
using Gip.Base;
using Gip.Base.Collections;
using Gip.Core;

namespace Gib.Base.IO
{

    public class ReadFile : FunctionContextBase
    {

        /// <summary>
        /// Gets the schema for the function.
        /// </summary>
        public override FunctionSchema Schema { get; } = FunctionSchema.CreateBuilder()
            .Source<ValueSignal<AbsoluteFile>>()
            .Output<SequenceSignal<byte>>()
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
            var fileData = call.Outputs[0].EmitSequence<byte>();
            var messages = call.Outputs[1].EmitSequence<string>();

            await foreach (var filePath in call.Sources[0].CollectValue<AbsoluteFile>(cancellationToken))
            {
                fileData.Clear();
                messages.Clear();

                try
                {
                    fileData.AppendRange(await File.ReadAllBytesAsync(filePath.AbsolutePath, cancellationToken));
                }
                catch (IOException e)
                {
                    fileData.Clear();
                    messages.Append(e.Message);
                }
            }
        }

    }

}
