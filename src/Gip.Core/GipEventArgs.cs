using System;

namespace Gip.Core
{

    /// <summary>
    /// Represents an event in a Gip pipeline.
    /// </summary>
    public abstract class GipEventArgs : EventArgs
    {

        readonly GipObject target;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target"></param>
        protected GipEventArgs(GipObject target)
        {
            this.target = target;
        }

        /// <summary>
        /// Gets the target of the event.
        /// </summary>
        public GipObject Target => target;

    }

}
