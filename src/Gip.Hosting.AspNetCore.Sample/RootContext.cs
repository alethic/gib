using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Gib.Base.IO;

using Gip.Abstractions;
using Gip.Base;
using Gip.Base.Collections;
using Gip.Core;

namespace Gip.Hosting.AspNetCore.Sample
{

    public class RootContext : FunctionContextBase
    {

        /// <summary>
        /// Gets the schema for the function.
        /// </summary>
        public override FunctionSchema Schema { get; } = FunctionSchema.CreateBuilder().Build();

        /// <summary>
        /// Handles an individual call.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task CallAsync(ICallContext context, CancellationToken cancellationToken)
        {
            var messagesChannel = context.Pipeline.CreateChannel(ChannelSchema.FromClrType<SequenceSignal<string>>());
            var fileListFunc = context.Pipeline.CreateFunction(new FileList());
            var fileFilterFunc = context.Pipeline.CreateFunction(new FileListFilter());
            var fileListToFileChannelFunc = context.Pipeline.CreateFunction(new DelegateSetToMap<AbsoluteFile, AbsoluteFile, ImmutableArray<IReadableChannelHandle>>(
                async (c, s, ct) => s,
                async (c, s, ct) => [c.Pipeline.CreateSingletonValue(s)]));
            var repeatFileFunc = context.Pipeline.CreateFunction(new RepeatFunction<AbsoluteFile>());
            var readFileFunc = context.Pipeline.CreateFunction(new ReadFile());
            var decodeTextFunc = context.Pipeline.CreateFunction(new DecodeText());
            var csharpCompilerFunc = context.Pipeline.CreateFunction(new CSharpCompilerContext());

            // call file tree to read sources directory
            var sourcesFileListDirectoryChannel = context.Pipeline.CreateSingletonValue(AbsoluteFile.FromPath("C:\\Users\\jhaltom\\temp"));
            using var sourcesFileListCall = await fileListFunc.CallAsync([sourcesFileListDirectoryChannel], cancellationToken);

            // filter file tree to only CS files
            var sourcesFileGlobChannel = context.Pipeline.CreateSingletonValue("*.cs");
            using var sourcesFileFilterCall = await fileFilterFunc.CallAsync([sourcesFileListCall.Outputs[0], sourcesFileGlobChannel], cancellationToken);

            // returns a map of AbsoluteFile to a channel containing an absolute file
            using var sourcesFileToMapOfChannelCall = await fileListToFileChannelFunc.CallAsync([sourcesFileFilterCall.Outputs[0]], cancellationToken);

            // repeat a repeater that creates a ReadFile function for each source file
            using var sourcesFileReadCall = await repeatFileFunc.CallAsync([context.Pipeline.CreateSingletonValue((IFunctionHandle)readFileFunc), sourcesFileToMapOfChannelCall.Outputs[0]], cancellationToken);

            var sourcesFileReadToDecodeCall = context.Pipeline.CreateFunction(new DelegateMapMap<AbsoluteFile, ImmutableArray<IReadableChannelHandle>, AbsoluteFile, ImmutableArray<IReadableChannelHandle>>(
                async (c, s, ct) => s,
                async (c, s, ct) => [s[0], s[1]]));

            // repeat a repeater that creates a DecodeText function for each source file
            using var sourcesFileDecodeCall = await repeatFileFunc.CallAsync([context.Pipeline.CreateSingletonValue((IFunctionHandle)decodeTextFunc), sourcesFileReadCall.Outputs[0]], cancellationToken);

            // call file tree to read references directory
            var refsFileListDirectoryChannel = context.Pipeline.CreateSingletonValue(AbsoluteFile.FromPath("C:\\Program Files\\dotnet\\packs\\Microsoft.NETCore.App.Ref\\10.0.1\\ref\\net10.0"));
            using var refsFileListCall = await fileListFunc.CallAsync([refsFileListDirectoryChannel], cancellationToken);

            // filter file tree to only CS files
            var refsFileGlobChannel = context.Pipeline.CreateSingletonValue("*.dll");
            using var refsFileFilterCall = await fileFilterFunc.CallAsync([refsFileListCall.Outputs[0], refsFileGlobChannel], cancellationToken);

            // returns a map of AbsoluteFile to a channel containing an absolute file
            using var refsFileToMapOfChannelCall = await fileListToFileChannelFunc.CallAsync([refsFileFilterCall.Outputs[0]], cancellationToken);

            // repeat a repeater that creates a ReadFile function for each source file
            using var refsFileReadCall = await repeatFileFunc.CallAsync([context.Pipeline.CreateSingletonValue((IFunctionHandle)readFileFunc), refsFileToMapOfChannelCall.Outputs[0]], cancellationToken);

            // run compiler
            var csharpAssemblyNameChannel = context.Pipeline.CreateSingletonValue("Test");
            var csharpOutputFileChannel = context.Pipeline.CreateSingletonValue(AbsoluteFile.FromPath("C:\\Users\\jhaltom\\Test.dll"));
            using var csharpCompilerCall = await csharpCompilerFunc.CallAsync([csharpAssemblyNameChannel, refsFileFilterCall.Outputs[0], sourcesFileFilterCall.Outputs[0], csharpOutputFileChannel], cancellationToken);

            var collectMessages = context.Pipeline.CreateFunction(new ConcatSequenceSet<string>());
            using var collectMessagesCall = collectMessages.CallAsync([], cancellationToken);

            await foreach (var messages in csharpCompilerCall.Outputs[0].CollectSequence<string>(cancellationToken))
            {
                Console.WriteLine();
                Console.WriteLine();
                foreach (var message in messages)
                    Console.WriteLine("Message: {0}", message);
            }

            //int i = 0;

            //// function to receive reference to operator func
            //var recvFunc = call.Pipeline.CreateFunction(new ReceiveFuncContext());

            //// channel to send input to receive function
            //var opChan = call.Pipeline.CreateChannel(recvFunc.Schema.Sources[0]);
            //var xChan = call.Pipeline.CreateChannel(recvFunc.Schema.Sources[1]);
            //var yChan = call.Pipeline.CreateChannel(recvFunc.Schema.Sources[2]);

            //// channel on which to receive results from op
            //var opResultChan = call.Pipeline.CreateChannel(recvFunc.Schema.Outputs[0]);

            //// initiate call to receive function, which receives the operator, and writes the results to the result chan
            //using var opCall = await recvFunc.CallAsync([opChan, xChan, yChan], [opResultChan], cancellationToken);

            //// writer to send function reference value to receiver
            //using var opWriter = opChan.EmitValue<FunctionReference>();

            //// channel and writer to write X value
            //using var xWriter = xChan.EmitValue<int>();

            //// channel and writer to write Y value
            //using var yWriter = yChan.EmitValue<int>();

            //// periodically change operator func
            //var fun = Task.Run(async () =>
            //{
            //    while (cancellationToken.IsCancellationRequested == false)
            //    {
            //        if (i++ % 2 == 0)
            //            opWriter.Set(call.Pipeline.GetFunctionReference(call.Pipeline.CreateFunction(new AdderContext())));
            //        else
            //            opWriter.Set(call.Pipeline.GetFunctionReference(call.Pipeline.CreateFunction(new MultiContext())));

            //        await Task.Delay(10000, cancellationToken);
            //    }
            //});

            //var val = Task.Run(async () =>
            //{
            //    while (cancellationToken.IsCancellationRequested == false)
            //    {
            //        // send new random X value
            //        xWriter.Set(Random.Shared.Next());

            //        // send new random Y value
            //        yWriter.Set(Random.Shared.Next());

            //        // wait a second before updating value
            //        await Task.Delay(1000, cancellationToken);
            //    }
            //});

            //var ret = Task.Run(async () =>
            //{
            //    await foreach (var i in opResultChan.CollectValue<int>(cancellationToken))
            //        System.Console.WriteLine(i);
            //});

            //await Task.WhenAll(fun, val, ret);
        }

    }

}