using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Gip.Abstractions;
using Gip.Base;
using Gip.Base.Collections;
using Gip.Core;

namespace Gib.Base.IO
{

    /// <summary>
    /// This elements reads from a source directory and returns the set of files.
    /// </summary>
    public class FileList : FunctionContextBase
    {

        /// <summary>
        /// Gets the schema for the function.
        /// </summary>
        public override FunctionSchema Schema { get; } = FunctionSchema.CreateBuilder()
            .Source<ValueSignal<AbsoluteFile>>()
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
            using var files = call.Outputs[0].EmitSet<AbsoluteFile>();

            await foreach (var dir in call.Sources[0].CollectValue<AbsoluteFile>(cancellationToken))
            {
                // directory changes, clear our list
                files.Clear();

                if (Directory.Exists(dir.AbsolutePath))
                {
                    // local map to model changes
                    var state = new Dictionary<string, AbsoluteFile>();

                    // create a watcher and send all events to channel
                    var channel = Channel.CreateUnbounded<EventArgs>();
                    var writer = channel.Writer;
                    var reader = channel.Reader;

                    using var watcher = new FileSystemWatcher(dir.AbsolutePath);
                    watcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Size;
                    watcher.IncludeSubdirectories = true;
                    watcher.Changed += (_, a) => writer.TryWrite(a);
                    watcher.Created += (_, a) => writer.TryWrite(a);
                    watcher.Deleted += (_, a) => writer.TryWrite(a);
                    watcher.Renamed += (_, a) => writer.TryWrite(a);
                    watcher.Error += (_, a) => writer.TryWrite(a);
                    watcher.EnableRaisingEvents = true;

                    // fill up a local hashset with the items as they stand now
                    foreach (var i in Directory.EnumerateFiles(dir.AbsolutePath, "*", new EnumerationOptions() { RecurseSubdirectories = true }))
                    {
                        var f = new FileInfo(i);
                        if (f.Exists)
                            state[f.FullName] = AbsoluteFile.FromPath(f.FullName);
                    }

                    // signal the addition of all existing items
                    files.AddRange(state.Values);

                    // read until we are cancelled
                    await foreach (var e in reader.ReadAllAsync(cancellationToken))
                    {
                        switch (e)
                        {
                            // rename events are a separate type that contain the file system info
                            case RenamedEventArgs renamed:
                                {
                                    // if the file was removed from the old path
                                    if (state.Remove(renamed.OldFullPath, out var oldFile))
                                        files.Remove(oldFile);

                                    // add file at new path
                                    var file = new FileInfo(renamed.FullPath);
                                    if (file.Exists)
                                    {
                                        var newFile = AbsoluteFile.FromPath(file.FullName);
                                        if (state.TryAdd(file.FullName, newFile))
                                            files.Add(newFile);
                                    }
                                }

                                break;

                            // all other events are handled with this type
                            case FileSystemEventArgs args:
                                {
                                    switch (args.ChangeType)
                                    {
                                        case WatcherChangeTypes.Created:
                                            {
                                                var file = new FileInfo(args.FullPath);
                                                if (file.Exists)
                                                {
                                                    var newFile = AbsoluteFile.FromPath(file.FullName);
                                                    if (state.TryAdd(file.FullName, newFile))
                                                        files.Add(newFile);
                                                }
                                                else
                                                {
                                                    if (state.Remove(file.FullName, out var oldFile))
                                                        files.Remove(oldFile);
                                                }
                                            }
                                            break;
                                        case WatcherChangeTypes.Deleted:
                                            {
                                                var file = new FileInfo(args.FullPath);
                                                if (file.Exists == false)
                                                    if (state.Remove(file.FullName, out var oldFile))
                                                        files.Remove(oldFile);
                                            }
                                            break;
                                        case WatcherChangeTypes.Changed:
                                            {
                                                var file = new FileInfo(args.FullPath);
                                                if (file.Exists)
                                                {
                                                    files.Freeze();

                                                    if (state.Remove(file.FullName, out var oldFile))
                                                        files.Remove(oldFile);

                                                    var newFile = AbsoluteFile.FromPath(file.FullName);
                                                    if (state.TryAdd(file.FullName, newFile))
                                                        files.Add(newFile);

                                                    files.Resume();
                                                }
                                                else
                                                {
                                                    if (state.Remove(file.FullName, out var oldFile))
                                                        files.Remove(oldFile);
                                                }
                                            }

                                            break;
                                        default:
                                            throw new InvalidOperationException();
                                    }
                                }

                                break;

                            case ErrorEventArgs exception:
                                ExceptionDispatchInfo.Capture(exception.GetException()).Throw();
                                throw null;
                            default:
                                throw new InvalidOperationException();
                        }
                    }
                }
            }
        }

    }

}