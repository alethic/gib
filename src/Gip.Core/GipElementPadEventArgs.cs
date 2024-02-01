namespace Gip.Core
{

    /// <summary>
    /// Describes the arguments of an event related to an element's pad.
    /// </summary>
    public class GipElementPadEventArgs : GipElementEventArgs
    {

        readonly GipElementPadEventType eventType;
        readonly GibPad pad;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="eventType"></param>
        /// <param name="pad"></param>
        public GipElementPadEventArgs(GipElement element, GipElementPadEventType eventType, GibPad pad) :
            base(element)
        {
            this.eventType = eventType;
            this.pad = pad ?? throw new System.ArgumentNullException(nameof(pad));
        }

        /// <summary>
        /// Describes the type of event.
        /// </summary>
        public GipElementPadEventType EventType => eventType;

        /// <summary>
        /// Gets the <see cref="GibPad"/> that this event is related to.
        /// </summary>
        public GibPad Pad => pad;

    }

}
