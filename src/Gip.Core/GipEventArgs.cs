using System;

namespace Gip.Core
{

    /// <summary>
    /// Represents an event in a Gip pipeline.
    /// </summary>
    public abstract class GipEventArgs : EventArgs
    {

        readonly GipObject sender;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="sender"></param>
        protected GipEventArgs(GipObject sender)
        {
            this.sender = sender;
        }

        /// <summary>
        /// Gets the target of the event.
        /// </summary>
        public GipObject Target => sender;

    }

}
