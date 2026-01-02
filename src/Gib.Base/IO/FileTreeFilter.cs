using System;
using System.Collections.Immutable;
using System.Threading;
using System.Threading.Tasks;

using Gib.Base.Collections;
using Gib.Core;
using Gib.Core.Elements;

namespace Gib.Base.IO
{

    /// <summary>
    /// This element reads from one file tree and outputs to another, applying a filter along the way.
    /// </summary>
    [Element]
    public class FileTreeFilter : ElementBase
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        public FileTreeFilter(IElementContext context) :
            base(context)
        {

        }

        /// <summary>
        /// Files to be filtered.
        /// </summary>
        [Property("sourceFiles")]
        public required IStreamConsumer<SetEvent<RelativeFile>> SourceFiles { get; set; }

        /// <summary>
        /// Result of applying filter.
        /// </summary>
        [Property("outputFiles")]
        public required IStreamProducer<SetEvent<RelativeFile>> OutputFiles { get; set; }

        /// <summary>
        /// Glob to apply to the files.
        /// </summary>
        [Property("glob")]
        public required string Filter { get; set; }

        /// <inheritdoc />
        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            await OutputFiles.ResetAsync();

            // listen to source file events
            await SourceFiles.Stream(async @event =>
            {
                switch (@event)
                {
                    case SetAddEvent<RelativeFile> addEvent:
                        {
                            if (IsMatch(addEvent.Item))
                                await OutputFiles.SendAsync(new SetAddEvent<RelativeFile>(addEvent.Item));

                            break;
                        }
                    case SetAddManyEvent<RelativeFile> addManyEvent:
                        {
                            var filteredFiles = ImmutableHashSet.CreateBuilder<RelativeFile>();

                            foreach (var file in addManyEvent.Items)
                                if (IsMatch(file))
                                    filteredFiles.Add(file);

                            await OutputFiles.SendAsync(new SetAddManyEvent<RelativeFile>(filteredFiles.ToImmutable()));

                            break;
                        }
                    case SetRemoveEvent<RelativeFile> removeEvent:
                        {
                            if (IsMatch(removeEvent.Item))
                                await OutputFiles.SendAsync(new SetRemoveEvent<RelativeFile>(removeEvent.Item));

                            break;
                        }
                    case SetRemoveManyEvent<RelativeFile> removeManyEvent:
                        {
                            var filteredFiles = ImmutableHashSet.CreateBuilder<RelativeFile>();

                            foreach (var file in removeManyEvent.Items)
                                if (IsMatch(file))
                                    filteredFiles.Add(file);

                            await OutputFiles.SendAsync(new SetRemoveManyEvent<RelativeFile>(filteredFiles.ToImmutable()));

                            break;
                        }
                }
            }, cancellationToken);
        }

        bool IsMatch(RelativeFile item)
        {
            throw new NotImplementedException();
        }

    }

}