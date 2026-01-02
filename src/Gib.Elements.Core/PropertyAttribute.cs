using System;

namespace Gib.Core.Elements
{

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public class PropertyAttribute : Attribute
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="propertyName"></param>
        public PropertyAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }

        /// <summary>
        /// Gets or sets the name of the property.
        /// </summary>
        public string PropertyName { get; }

    }

}
