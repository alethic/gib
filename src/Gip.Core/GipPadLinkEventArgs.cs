namespace Gip.Core
{

    public class GipPadLinkEventArgs : GipPadEventArgs
    {

        readonly GipPadLinkEventType eventType;
        readonly GipPad? peer;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="peer"></param>
        public GipPadLinkEventArgs(GipPad pad, GipPadLinkEventType eventType, GipPad? peer) :
            base(pad)
        {
            this.eventType = eventType;
            this.peer = peer;
        }

        /// <summary>
        /// Gets the pad that raised this event.
        /// </summary>
        public new GipPad Target => (GipPad)base.Target;

        /// <summary>
        /// Gets the type of link event.
        /// </summary>
        public GipPadLinkEventType EventType => eventType;

        /// <summary>
        /// Gets the peer related to this event.
        /// </summary>
        public GipPad? Peer => peer;

    }

}
