using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using CliWrap;

using Gib.Base.IO;
using Gib.Core;
using Gib.Core.Elements;

namespace Gib.Base
{

    /// <summary>
    /// Element that executes a process and returns its standard output and error.
    /// </summary>
    [Element]
    public class Exec : ElementBase
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        public Exec(IElementContext context) :
            base(context)
        {

        }

        /// <summary>
        /// Optional version string used to cause rerun.
        /// </summary>
        [Property("version")]
        public string? Version { get; set; }

        /// <summary>
        /// Directory to watch.
        /// </summary>
        [Property("workingDirectory")]
        public DirectoryPath? WorkingDirectory { get; set; }

        /// <summary>
        /// Environment variables to pass to the process.
        /// </summary>
        [Property("env")]
        public IReadOnlyDictionary<string, string?>? EnvironmentVariables { get; set; }

        /// <summary>
        /// Name of the executable to be run.
        /// </summary>
        [Property("executable")]
        public required FilePath Executable { get; set; }

        /// <summary>
        /// Set of files that have been copied.
        /// </summary>
        [Property("args")]
        public required string[] Args { get; set; }

        /// <summary>
        /// Gets the standard input of the process.
        /// </summary>
        [Property("stdin")]
        public byte[]? StandardIn { get; set; }

        /// <summary>
        /// Gets the standard output of the process.
        /// </summary>
        [Property("stdout")]
        public IValueProducer<byte[]>? StandardOut { get; set; }

        /// <summary>
        /// Gets the standard error of the process.
        /// </summary>
        [Property("stderr")]
        public IValueProducer<byte[]>? StandardErr { get; set; }

        /// <summary>
        /// Gets the exit code of the process.
        /// </summary>
        [Property("exitCode")]
        public int? ExitCode { get; private set; }

        /// <summary>
        /// Gets the time the execution took.
        /// </summary>
        [Property("execTime")]
        public TimeSpan? ExecTime { get; private set; }

        /// <inheritdoc />
        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var cli = Cli.Wrap(Executable.AbsolutePath).WithArguments(Args);

            // disable built in validation
            cli = cli.WithValidation(CommandResultValidation.None);

            // set working directory if specified
            if (WorkingDirectory is not null)
                cli = cli.WithWorkingDirectory(WorkingDirectory.Value.AbsolutePath);

            // set environment variables if specified
            if (EnvironmentVariables is not null)
                cli = cli.WithEnvironmentVariables(EnvironmentVariables);

            // send standard input to command line if specified
            if (StandardIn is not null)
                cli = cli.WithStandardInputPipe(PipeSource.FromBytes(StandardIn));

            // capture stdout if required
            MemoryStream? stdout = null;
            if (StandardOut is not null)
            {
                stdout = new MemoryStream();
                cli = cli.WithStandardOutputPipe(PipeTarget.ToStream(stdout));
            }

            // capture stderr if required
            MemoryStream? stderr = null;
            if (StandardErr is not null)
            {
                stderr = new MemoryStream();
                cli = cli.WithStandardOutputPipe(PipeTarget.ToStream(stderr));
            }

            // execute process
            var result = await cli.ExecuteAsync(cancellationToken: cancellationToken);

            // set output exit code
            ExitCode = result.ExitCode;
            ExecTime = result.RunTime;

            // if capturing stdout, set as output
            if (stdout is not null && StandardOut is not null)
                await StandardOut.SetAsync(stdout.ToArray());

            // if capturing stderr, set as output
            if (stderr is not null && StandardErr is not null)
                await StandardErr.SetAsync(stderr.ToArray());
        }

    }

}
