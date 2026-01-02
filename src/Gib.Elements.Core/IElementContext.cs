using System;
using System.Threading.Tasks;

namespace Gib.Core.Elements
{

    public interface IElementContext
    {

        /// <summary>
        /// Gets a reference to the root element.
        /// </summary>
        Uri Root { get; }

        /// <summary>
        /// Gets a reference to the current element.
        /// </summary>
        Uri Self { get; }

        /// <summary>
        /// Registers the given element type and returns a reference to it.
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <param name="init"></param>
        /// <returns></returns>
        Uri RegisterElementType<TElement>()
            where TElement : IElement;

        /// <summary>
        /// Registers the given element type and returns a reference to it.
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        Uri RegisterElementType<TElement>(ReadOnlySpan<object> args)
            where TElement : IElement;

        /// <summary>
        /// Creates a new element which will be released when the context ends.
        /// </summary>
        /// <typeparam name="TProxy"></typeparam>
        /// <returns></returns>
        TProxy CreateElement<TProxy, TElement>()
            where TProxy : IElementProxy
            where TElement : IElementWithProxy<TProxy>;

        /// <summary>
        /// Creates a new element which will be released when the context ends.
        /// </summary>
        /// <typeparam name="TProxy"></typeparam>
        /// <param name="args"></param>
        /// <returns></returns>
        TProxy CreateElement<TProxy, TElement>(ReadOnlySpan<object> args)
            where TProxy : IElementProxy
            where TElement : IElementWithProxy<TProxy>;

        /// <summary>
        /// Creates a new element which will be released when the context ends.
        /// </summary>
        /// <typeparam name="TProxy"></typeparam>
        /// <param name="elementTypeUri"></param>
        /// <returns></returns>
        TProxy CreateElement<TProxy>(Uri elementTypeUri)
            where TProxy : IElementProxy;

        /// <summary>
        /// Creates a new element which will be released when the context ends.
        /// </summary>
        /// <typeparam name="TProxy"></typeparam>
        /// <param name="elementTypeUri"></param>
        /// <returns></returns>
        TProxy CreateElement<TProxy>(IValueBinding<Uri> elementTypeUri)
            where TProxy : IElementProxy;

        /// <summary>
        /// Gets a reference to an existing element which will be released when the context ends.
        /// </summary>
        /// <typeparam name="TProxy"></typeparam>
        /// <param name="elementUri"></param>
        /// <returns></returns>
        TProxy GetElement<TProxy>(Uri elementUri)
            where TProxy : IElementProxy;

        /// <summary>
        /// Gets the current property value of the current element.
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        string GetPropertyValue<T>(string v);

        /// <summary>
        /// Allocates a constant value binding.
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        IValueBinding<T> ValueOf<T>(T value);

    }

}