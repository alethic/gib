using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Gib.Base.Collections;
using Gib.Base.IO;
using Gib.Core;
using Gib.Core.Elements;

namespace Gib.Console
{

    public class TemplateElement<TSource> : ElementBase
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        public TemplateElement(IElementContext context) :
            base(context)
        {

        }

        /// <summary>
        /// List of items to result in the creation of an element.
        /// </summary>
        [Property("sourceItems")]
        public required IStreamConsumer<ListEvent<TSource>> SourceItems { get; set; }

        /// <summary>
        /// Type of the element to produce for each source item.
        /// </summary>
        [Property("elementType")]
        public required ElementTypeReference ElementType { get; set; }

        /// <summary>
        /// List of elements that are produced. These are in 1:1 correspondence with the source items.
        /// </summary>
        [Property("elements")]
        public required IStreamProducer<ListEvent<ElementReference>> Elements { get; set; }

        /// <inheritdoc />
        public override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            // we are replaying all the elements
            await Elements.ResetAsync();

            // read from source files, transform into output element set
            await foreach (var @event in SourceItems.WithCancellation(cancellationToken))
                await ProcessSourceItemEvent(@event, cancellationToken);
        }

        Task ProcessSourceItemEvent(ListEvent<TSource> @event, CancellationToken cancellationToken) => @event switch
        {
            ListInsertEvent<FilePath> insertEvent => ProcessListInsertEvent(insertEvent, cancellationToken),
            ListInsertManyEvent<FilePath> insertManyEvent => ProcessSourceItemInsertManyEvent(insertManyEvent, cancellationToken),
            ListRemoveEvent<FilePath> removeEvent => ProcessSourceItemRemoveEvent(removeEvent, cancellationToken),
            ListRemoveManyEvent<FilePath> removeManyEvent => ProcessSourceItemRemoveManyEvent(removeManyEvent, cancellationToken),
            ListSetEvent<FilePath> setEvent => ProcessSourceItemSetEvent(setEvent, cancellationToken),
            ListSetManyEvent<FilePath> setManyEvent => ProcessSourceItemSetManyEvent(setManyEvent, cancellationToken),
            _ => throw new InvalidOperationException(),
        };

        async Task ProcessListInsertEvent(ListInsertEvent<FilePath> evnt, CancellationToken cancellationToken)
        {
            var element = CreateElement<IElementProxy>(ElementType.Uri);
            await Elements.SendAsync(new ListInsertEvent<ElementReference>(evnt.Index, new ElementReference(element.ElementUri)));
        }

        async Task ProcessSourceItemInsertManyEvent(ListInsertManyEvent<FilePath> insertManyEvent, CancellationToken cancellationToken)
        {
            var elements = ImmutableArray.CreateBuilder<ElementReference>(insertManyEvent.Items.Length);

            foreach (var item in insertManyEvent.Items)
            {
                var element = CreateElement<IElementProxy>(ElementType.Uri);
                elements.Add(new ElementReference(element.ElementUri));
            }

            await Elements.SendAsync(new ListInsertManyEvent<ElementReference>(insertManyEvent.Index, elements.ToImmutable()));
        }

        async Task ProcessSourceItemRemoveEvent(ListRemoveEvent<FilePath> removeEvent, CancellationToken cancellationToken)
        {
            await Elements.SendAsync(new ListRemoveEvent<ElementReference>(removeEvent.Index));
        }

        async Task ProcessSourceItemRemoveManyEvent(ListRemoveManyEvent<FilePath> removeManyEvent, CancellationToken cancellationToken)
        {
            await Elements.SendAsync(new ListRemoveManyEvent<ElementReference>(removeManyEvent.Range));
        }

        async Task ProcessSourceItemSetEvent(ListSetEvent<FilePath> setEvent, CancellationToken cancellationToken)
        {
            var element = CreateElement<IElementProxy>(ElementType.Uri);
            await Elements.SendAsync(new ListSetEvent<ElementReference>(setEvent.Index, new ElementReference(element.ElementUri)));
        }

        async Task ProcessSourceItemSetManyEvent(ListSetManyEvent<FilePath> setManyEvent, CancellationToken cancellationToken)
        {
            var elements = ImmutableArray.CreateBuilder<ElementReference>(setManyEvent.Items.Length);

            foreach (var item in setManyEvent.Items)
            {
                var element = CreateElement<IElementProxy>(ElementType.Uri);
                elements.Add(new ElementReference(element.ElementUri));
            }

            await Elements.SendAsync(new ListSetManyEvent<ElementReference>(setManyEvent.Index, elements.ToImmutable()));
        }

    }

}
