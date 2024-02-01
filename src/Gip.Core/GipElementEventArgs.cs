using System;

namespace Gip.Core
{

    public abstract class GipElementEventArgs : EventArgs
    {

        readonly GipElement element;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="element"></param>
        public GipElementEventArgs(GipElement element)
        {
            this.element = element ?? throw new ArgumentNullException(nameof(element));
        }

        /// <summary>
        /// Gets the element that raised this event.
        /// </summary>
        public GipElement Element => element;

    }

}
