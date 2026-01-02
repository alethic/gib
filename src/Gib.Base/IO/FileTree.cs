using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

using Gib.Base.Collections;
using Gib.Core;
using Gib.Core.Elements;

namespace Gib.Base.IO
{

    /// <summary>
    /// This elements reads from a source directory and returns the set of files.
    /// </summary>
    [Element]
    public class FileTree : ElementBase
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        public FileTree(IElementContext context) :
            base(context)
        {

        }

        /// <summary>
        /// Directory to watch.
        /// </summary>
        [Property("directory")]
        public required DirectoryPath Directory { get; set; }

        /// <summary>
        /// Set of files in the directory.
        /// </summary>
        [Property("files")]
        public required IStreamProducer<SetEvent<RelativeFile>> Files { get; set; }

        /// <inheritdoc />
        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                // we will be replaying all of the files
                await Files.ResetAsync();

                if (System.IO.Directory.Exists(Directory.AbsolutePath))
                {
                    // local map to model changes
                    var state = new Dictionary<string, RelativeFile>();

                    // create a watcher and send all events to channel
                    var channel = Channel.CreateUnbounded<EventArgs>();
                    var writer = channel.Writer;
                    var reader = channel.Reader;

                    using var watcher = new FileSystemWatcher(Directory.AbsolutePath);
                    watcher.NotifyFilter = NotifyFilters.Attributes | NotifyFilters.DirectoryName | NotifyFilters.FileName | NotifyFilters.CreationTime | NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.Size;
                    watcher.IncludeSubdirectories = true;
                    watcher.Changed += (_, a) => writer.TryWrite(a);
                    watcher.Created += (_, a) => writer.TryWrite(a);
                    watcher.Deleted += (_, a) => writer.TryWrite(a);
                    watcher.Renamed += (_, a) => writer.TryWrite(a);
                    watcher.Error += (_, a) => writer.TryWrite(a);
                    watcher.EnableRaisingEvents = true;

                    // fill up a local hashset with the items as they stand now
                    foreach (var i in System.IO.Directory.EnumerateFiles(Directory.AbsolutePath, "*", new EnumerationOptions() { RecurseSubdirectories = true }))
                    {
                        var f = new FileInfo(i);
                        if (f.Exists)
                            state[i] = new RelativeFile(i, Path.GetRelativePath(Directory.AbsolutePath, i));
                    }

                    // signal the addition of all existing items
                    await Files.SendAsync(new SetAddManyEvent<RelativeFile>(state.Values.ToImmutableHashSet()));

                    // read until we are cancelled
                    await foreach (var e in reader.ReadAllAsync(cancellationToken))
                    {
                        switch (e)
                        {
                            // rename events are a separate type that contain the file system info
                            case RenamedEventArgs renamed:
                                {
                                    // if the file was removed from the old path
                                    if (state.Remove(renamed.OldFullPath, out var relativeFile))
                                        await Files.SendAsync(new SetRemoveEvent<RelativeFile>(relativeFile));

                                    // add file at new path
                                    var file = new FileInfo(renamed.FullPath);
                                    if (file.Exists)
                                    {
                                        relativeFile = new RelativeFile(file.FullName, Path.GetRelativePath(Directory.AbsolutePath, file.FullName));
                                        if (state.TryAdd(file.FullName, relativeFile))
                                            await Files.SendAsync(new SetAddEvent<RelativeFile>(relativeFile));
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
                                                    var relativeFile = new RelativeFile(file.FullName, Path.GetRelativePath(Directory.AbsolutePath, file.FullName));
                                                    if (state.TryAdd(file.FullName, relativeFile))
                                                        await Files.SendAsync(new SetAddEvent<RelativeFile>(relativeFile));
                                                }
                                                else
                                                {
                                                    if (state.Remove(file.FullName, out var relativeFile))
                                                        await Files.SendAsync(new SetRemoveEvent<RelativeFile>(relativeFile));
                                                }
                                            }
                                            break;
                                        case WatcherChangeTypes.Deleted:
                                            {
                                                var file = new FileInfo(args.FullPath);
                                                if (file.Exists == false)
                                                    if (state.Remove(file.FullName, out var relativeFile))
                                                        await Files.SendAsync(new SetRemoveEvent<RelativeFile>(relativeFile));
                                            }
                                            break;
                                        case WatcherChangeTypes.Changed:
                                            {
                                                var file = new FileInfo(args.FullPath);
                                                if (file.Exists)
                                                {
                                                    if (state.Remove(file.FullName, out var relativeFile))
                                                        await Files.SendAsync(new SetRemoveEvent<RelativeFile>(relativeFile));

                                                    relativeFile = new RelativeFile(file.FullName, Path.GetRelativePath(Directory.AbsolutePath, file.FullName));
                                                    if (state.TryAdd(file.FullName, relativeFile))
                                                        await Files.SendAsync(new SetAddEvent<RelativeFile>(relativeFile));
                                                }
                                                else
                                                {
                                                    if (state.Remove(file.FullName, out var relativeFile))
                                                        await Files.SendAsync(new SetRemoveEvent<RelativeFile>(relativeFile));
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
            catch (OperationCanceledException)
            {
                // ignore
            }
        }

    }

}