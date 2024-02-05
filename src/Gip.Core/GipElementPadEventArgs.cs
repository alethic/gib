using System;

namespace Gip.Core
{

    /// <summary>
    /// Describes the arguments of an event related to an element's pad.
    /// </summary>
    public class GipElementPadEventArgs : GipElementEventArgs
    {

        readonly GipElementPadEventType eventType;
        readonly GipPad pad;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="eventType"></param>
        /// <param name="pad"></param>
        public GipElementPadEventArgs(GipElement target, GipElementPadEventType eventType, GipPad pad) :
            base(target)
        {
            this.eventType = eventType;
            this.pad = pad ?? throw new ArgumentNullException(nameof(pad));
        }

        /// <summary>
        /// Describes the type of event.
        /// </summary>
        public GipElementPadEventType EventType => eventType;

        /// <summary>
        /// Gets the <see cref="GibPad"/> that this event is related to.
        /// </summary>
        public GipPad Pad => pad;

    }

}
