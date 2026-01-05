using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Gib.Base.IO;

using Gip.Abstractions;
using Gip.Base;
using Gip.Base.Collections;
using Gip.Core;

using Google.Protobuf.WellKnownTypes;

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
        /// <param name="call"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task CallAsync(ICallContext call, CancellationToken cancellationToken)
        {
            var fileListFunc = call.Pipeline.CreateFunction(new FileList());
            var fileFilterFunc = call.Pipeline.CreateFunction(new FileListFilter());
            var csharpCompilerFunc = call.Pipeline.CreateFunction(new CSharpCompilerContext());

            // call file tree to read sources directory
            var sourcesFileListDirectoryChannel = call.Pipeline.CreateChannel(ChannelSchema.FromClrType<ValueSignal<AbsoluteFile>>());
            using var sourcesFileListDirectoryEmit = sourcesFileListDirectoryChannel.EmitValue<AbsoluteFile>();
            sourcesFileListDirectoryEmit.Set(AbsoluteFile.FromPath("C:\\Users\\jhaltom\\temp"));
            var sourcesFileListFilesChannel = call.Pipeline.CreateChannel(ChannelSchema.FromClrType<SetSignal<AbsoluteFile>>());
            using var sourcesFileListCall = await fileListFunc.CallAsync([sourcesFileListDirectoryChannel], [sourcesFileListFilesChannel], cancellationToken);

            // filter file tree to only CS files
            var sourcesFileListFilterGlobChannel = call.Pipeline.CreateChannel(ChannelSchema.FromClrType<ValueSignal<string>>());
            using var sourcesFileListFilterGlobEmit = sourcesFileListFilterGlobChannel.EmitValue<string>();
            sourcesFileListFilterGlobEmit.Set("*.cs");
            var sourcesFileListFilterOutputChannel = call.Pipeline.CreateChannel(ChannelSchema.FromClrType<SetSignal<AbsoluteFile>>());
            using var sourcesFileTreeFilterCall = await fileFilterFunc.CallAsync([sourcesFileListFilesChannel, sourcesFileListFilterGlobChannel], [sourcesFileListFilterOutputChannel], cancellationToken);

            // call file tree to read references directory
            var refsFileListDirectoryChannel = call.Pipeline.CreateChannel(ChannelSchema.FromClrType<ValueSignal<AbsoluteFile>>());
            using var refsFileListDirectoryEmit = refsFileListDirectoryChannel.EmitValue<AbsoluteFile>();
            refsFileListDirectoryEmit.Set(AbsoluteFile.FromPath("C:\\Program Files\\dotnet\\packs\\Microsoft.NETCore.App.Ref\\10.0.1\\ref\\net10.0"));
            var refsFileListFilesChannel = call.Pipeline.CreateChannel(ChannelSchema.FromClrType<SetSignal<AbsoluteFile>>());
            using var refsFileListCall = await fileListFunc.CallAsync([refsFileListDirectoryChannel], [refsFileListFilesChannel], cancellationToken);

            // filter file tree to only CS files
            var refsFileListFilterGlobChannel = call.Pipeline.CreateChannel(ChannelSchema.FromClrType<ValueSignal<string>>());
            using var refsFileListFilterGlobEmit = refsFileListFilterGlobChannel.EmitValue<string>();
            refsFileListFilterGlobEmit.Set("*.dll");
            var refsFileListFilterOutputChannel = call.Pipeline.CreateChannel(ChannelSchema.FromClrType<SetSignal<AbsoluteFile>>());
            using var refsFileTreeFilterCall = await fileFilterFunc.CallAsync([refsFileListFilesChannel, refsFileListFilterGlobChannel], [refsFileListFilterOutputChannel], cancellationToken);

            // run compiler
            var csharpAssemblyNameChannel = call.Pipeline.CreateChannel(ChannelSchema.FromClrType<ValueSignal<string>>());
            using var csharpAssemblyNameEmit = csharpAssemblyNameChannel.EmitValue<string>();
            csharpAssemblyNameEmit.Set("Test");
            var csharpOutputFileChannel = call.Pipeline.CreateChannel(ChannelSchema.FromClrType<ValueSignal<AbsoluteFile>>());
            using var csharpOutputFileEmit = csharpOutputFileChannel.EmitValue<AbsoluteFile>();
            csharpOutputFileEmit.Set(AbsoluteFile.FromPath("C:\\Users\\jhaltom\\Test.dll"));
            var csharpMessageChannel = call.Pipeline.CreateChannel(ChannelSchema.FromClrType<SequenceSignal<string>>());
            using var csharpCompilerCall = await csharpCompilerFunc.CallAsync([csharpAssemblyNameChannel, refsFileListFilterOutputChannel, sourcesFileListFilterOutputChannel, csharpOutputFileChannel], [csharpMessageChannel], cancellationToken);

            await foreach (var messages in csharpMessageChannel.CollectSequence<string>(cancellationToken))
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