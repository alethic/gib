using System;

namespace Gib.Core.Elements
{

    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class ElementAttribute : Attribute
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ElementAttribute()
        {

        }

    }

}
