using System;

using Gip.Abstractions;

namespace Gib.Core
{

    public class ElementReflector
    {

        /// <summary>
        /// Derives a <see cref="FunctionSchema"/> from the specified element type.
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <returns></returns>
        public ElementSchema GetElementSchema<TElement>()
            where TElement : IElement
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Derives a <see cref="FunctionSchema"/> from the specified element type.
        /// </summary>
        /// <typeparam name="TElement"></typeparam>
        /// <returns></returns>
        public FunctionSchema GetFunctionSchema<TElement>()
            where TElement : IElement
        {
            throw new NotImplementedException();
        }

    }

}
