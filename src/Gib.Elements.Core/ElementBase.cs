using System;
using System.Threading;
using System.Threading.Tasks;

namespace Gib.Core.Elements
{

    /// <summary>
    /// Base <see cref="IElement"/> implementation that provides some useful methods.
    /// </summary>
    public abstract class ElementBase : IElement
    {

        readonly IElementContext _context;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="context"></param>
        public ElementBase(IElementContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Gets the context that hosts this element.
        /// </summary>
        public IElementContext Context => _context;

        /// <summary>
        /// Gets the URI of the root element.
        /// </summary>
        public Uri Root => Context.Root;

        /// <summary>
        /// Invoked to execute the element.
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public abstract Task ExecuteAsync(CancellationToken cancellationToken);

        /// <summary>
        /// Registers the given element type and returns a reference to it.
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <param name="init"></param>
        /// <returns></returns>
        public Uri RegisterElementType<TElement>()
            where TElement : IElement
        {
            return Context.RegisterElementType<TElement>();
        }

        /// <summary>
        /// Registers the given element type and returns a reference to it.
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <typeparam name="TArg1"></typeparam>
        /// <param name="arg1"></param>
        /// <returns></returns>
        public Uri RegisterElementType<TElement, TArg1>(TArg1 arg1)
            where TElement : IElement
        {
            return Context.RegisterElementType<TElement, TArg1>(arg1);
        }

        /// <summary>
        /// Registers the given element type and returns a reference to it.
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <returns></returns>
        public Uri RegisterElementType<TElement, TArg1, TArg2>(TArg1 arg1, TArg2 arg2)
            where TElement : IElement
        {
            return Context.RegisterElementType<TElement, TArg1, TArg2>(arg1, arg2);
        }

        /// <summary>
        /// Registers the given element type and returns a reference to it.
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <typeparam name="TArg1"></typeparam>
        /// <typeparam name="TArg2"></typeparam
        /// <typeparam name="TArg3"></typeparam>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        /// <returns></returns>
        public Uri RegisterElementType<TElement, TArg1, TArg2, TArg3>(TArg1 arg1, TArg2 arg2, TArg3 arg3)
            where TElement : IElement
        {
            return Context.RegisterElementType<TElement, TArg1, TArg2, TArg3>(arg1, arg2, arg3);
        }

        /// <summary>
        /// Registers the given element type and returns a reference to it.
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        public Uri RegisterElementType<TElement>(ReadOnlySpan<object> args)
            where TElement : IElement
        {
            return Context.RegisterElementType<TElement>(args);
        }

        /// <summary>
        /// Creates a new element which will be released when the context ends.
        /// </summary>
        /// <typeparam name="TProxy"></typeparam>
        /// <returns></returns>
        public TProxy CreateElement<TProxy>()
            where TProxy : IElementProxy
        {
            return Context.CreateElement<TProxy>();
        }

        /// <summary>
        /// Creates a new element which will be released when the context ends.
        /// </summary>
        /// <typeparam name="TProxy"></typeparam>
        /// <param name="elementTypeUri"></param>
        /// <returns></returns>
        public TProxy CreateElement<TProxy>(Uri elementTypeUri)
            where TProxy : IElementProxy
        {
            return Context.CreateElement<TProxy>(elementTypeUri);
        }

        /// <summary>
        /// Creates a new element which will be released when the context ends.
        /// </summary>
        /// <typeparam name="TProxy"></typeparam>
        /// <param name="elementTypeUri"></param>
        /// <returns></returns>
        public TProxy CreateElement<TProxy>(IValueBinding<Uri> elementTypeUri)
            where TProxy : IElementProxy
        {
            return Context.CreateElement<TProxy>(elementTypeUri);
        }

        /// <summary>
        /// Registers the given element and returns a proxy to it.
        /// </summary>
        /// <typeparam name="TProxy"></typeparam>
        /// <param name="init"></param>
        /// <returns></returns>
        public TProxy CreateElement<TProxy>(Func<IElementContext, IElementWithProxy<TProxy>> init)
            where TProxy : IElementProxy
        {
            return Context.CreateElement(init);
        }

        /// <summary>
        /// Registers the given element and returns a proxy to it.
        /// </summary>
        /// <typeparam name="TProxy"></typeparam>
        /// <param name="init"></param>
        /// <returns></returns>
        TProxy CreateElement<TProxy>(Func<IElementContext, ValueTask<IElementWithProxy<TProxy>>> init)
            where TProxy : IElementProxy
        {
            return Context.CreateElement(init);
        }

        /// <summary>
        /// Gets a reference to an existing element which will be released when the context ends.
        /// </summary>
        /// <typeparam name="TProxy"></typeparam>
        /// <param name="elementUri"></param>
        /// <returns></returns>
        public TProxy GetElement<TProxy>(Uri elementUri)
            where TProxy : IElementProxy
        {
            return Context.GetElement<TProxy>(elementUri);
        }

        /// <summary>
        /// Allocates a constant value binding.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public IValueBinding<T> ValueOf<T>(T value)
        {
            return Context.ValueOf(value);
        }

        /// <summary>
        /// Invokes the specified action for each value change in either of the consumers.
        /// </summary>
        /// <typeparam name="TValue1"></typeparam>
        /// <typeparam name="TValue2"></typeparam>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task Join<TValue1, TValue2>(IValueConsumer<TValue1> value1, IValueConsumer<TValue2> value2, Func<TValue1?, TValue2?, Task> action, CancellationToken cancellationToken)
        {
            TValue1? v1 = default;
            TValue2? v2 = default;

            var t1 = value1.Listen(v => { v1 = v; action(v1, v2); }, cancellationToken);
            var t2 = value2.Listen(v => { v2 = v; action(v1, v2); }, cancellationToken);

            return Task.WhenAll([t1, t2]);
        }

        /// <summary>
        /// Invokes the specified action for each value change in either of the consumers.
        /// </summary>
        /// <typeparam name="TValue1"></typeparam>
        /// <typeparam name="TValue2"></typeparam>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task Join<TValue1, TValue2>(IValueBinding<TValue1> value1, IValueBinding<TValue2> value2, Func<TValue1?, TValue2?, Task> action, CancellationToken cancellationToken)
        {
            TValue1? v1 = default;
            TValue2? v2 = default;

            var t1 = value1.Listen(v => { v1 = v; action(v1, v2); }, cancellationToken);
            var t2 = value2.Listen(v => { v2 = v; action(v1, v2); }, cancellationToken);

            return Task.WhenAll([t1, t2]);
        }

        /// <summary>
        /// Invokes the specified action for each value change in either of the consumers.
        /// </summary>
        /// <typeparam name="TValue1"></typeparam>
        /// <typeparam name="TValue2"></typeparam>
        /// <typeparam name="TValue3"></typeparam>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <param name="value3"></param>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task Join<TValue1, TValue2, TValue3>(IValueConsumer<TValue1> value1, IValueConsumer<TValue2> value2, IValueConsumer<TValue3> value3, Func<TValue1?, TValue2?, TValue3?, Task> action, CancellationToken cancellationToken)
        {
            TValue1? v1 = default;
            TValue2? v2 = default;
            TValue3? v3 = default;

            var t1 = value1.Listen(v => { v1 = v; action(v1, v2, v3); }, cancellationToken);
            var t2 = value2.Listen(v => { v2 = v; action(v1, v2, v3); }, cancellationToken);
            var t3 = value3.Listen(v => { v3 = v; action(v1, v2, v3); }, cancellationToken);

            return Task.WhenAll([t1, t2, t3]);
        }

        /// <summary>
        /// Invokes the specified action for each value change in either of the consumers.
        /// </summary>
        /// <typeparam name="TValue1"></typeparam>
        /// <typeparam name="TValue2"></typeparam>
        /// <typeparam name="TValue3"></typeparam>
        /// <param name="value1"></param>
        /// <param name="value2"></param>
        /// <param name="value3"></param>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task Join<TValue1, TValue2, TValue3>(IValueBinding<TValue1> value1, IValueBinding<TValue2> value2, IValueBinding<TValue3> value3, Func<TValue1?, TValue2?, TValue3?, Task> action, CancellationToken cancellationToken)
        {
            TValue1? v1 = default;
            TValue2? v2 = default;
            TValue3? v3 = default;

            var t1 = value1.Listen(v => { v1 = v; action(v1, v2, v3); }, cancellationToken);
            var t2 = value2.Listen(v => { v2 = v; action(v1, v2, v3); }, cancellationToken);
            var t3 = value3.Listen(v => { v3 = v; action(v1, v2, v3); }, cancellationToken);

            return Task.WhenAll([t1, t2, t3]);
        }

    }

}
