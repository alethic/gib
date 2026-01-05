using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using Gib.Base.IO;

using Gip.Abstractions;
using Gip.Base;
using Gip.Base.Collections;
using Gip.Core;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Gip.Hosting.AspNetCore.Sample
{

    public class CSharpCompilerContext : FunctionContextBase
    {

        /// <summary>
        /// Gets the schema for the function.
        /// </summary>
        public override FunctionSchema Schema { get; } = FunctionSchema.CreateBuilder()
            .Source<ValueSignal<string>>()
            .Source<SetSignal<AbsoluteFile>>()
            .Source<SetSignal<AbsoluteFile>>()
            .Source<ValueSignal<AbsoluteFile>>()
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
            using var messages = call.Outputs[0].EmitSequence<string>();

            await foreach (var (assemblyName, referenceFiles, sourceFiles, outputFile) in AsyncEnumerableExtensions.Latest(
                call.Sources[0].CollectValue<string>(cancellationToken),
                call.Sources[1].CollectSet<AbsoluteFile>(cancellationToken),
                call.Sources[2].CollectSet<AbsoluteFile>(cancellationToken),
                call.Sources[3].CollectValue<AbsoluteFile>(cancellationToken),
                cancellationToken))
            {
                // clear existing log messages
                messages.Clear();

                var trees = new List<SyntaxTree>();
                foreach (var f in sourceFiles)
                {
                    if (File.Exists(f.AbsolutePath) == false)
                    {
                        messages.Append($"Source file not found: {f.AbsolutePath}.");
                        continue;
                    }

                    var tree = CSharpSyntaxTree.ParseText(File.ReadAllText(f.AbsolutePath), cancellationToken: cancellationToken);
                    foreach (var diag in tree.GetDiagnostics(cancellationToken))
                        messages.Append(diag.GetMessage());

                    trees.Add(tree);
                }

                var references = new List<MetadataReference>();
                foreach (var f in referenceFiles)
                {
                    if (File.Exists(f.AbsolutePath) == false)
                    {
                        messages.Append($"Refernce file not found: {f.AbsolutePath}.");
                        continue;
                    }

                    var reference = MetadataReference.CreateFromFile(f.AbsolutePath);
                    references.Add(reference);
                }

                var compilation = CSharpCompilation.Create(assemblyName, trees, references, new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));
                var result = compilation.Emit(outputFile.AbsolutePath, cancellationToken: cancellationToken);

                foreach (var diag in result.Diagnostics)
                    messages.Append(diag.GetMessage());
            }
        }

    }

}