using System;

namespace Gib.Core
{

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class ElementPropertyAttribute : Attribute
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="index"></param>
        public ElementPropertyAttribute(int index)
        {
            Index = index;
        }

        /// <summary>
        /// Gets the immutable index of the property.
        /// </summary>
        public int Index { get; set; }

    }

}
