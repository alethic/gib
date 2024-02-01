using System;

namespace Gip.Core
{

    public class GipPadLinkEventArgs : GipPadEventArgs
    {

        readonly GibPad pad;
        readonly GipPadLinkEventType eventType;
        readonly GibPad? peer;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="eventType"></param>
        /// <param name="peer"></param>
        public GipPadLinkEventArgs(GibPad pad, GipPadLinkEventType eventType, GibPad? peer)
        {
            this.pad = pad ?? throw new ArgumentNullException(nameof(pad));
            this.eventType = eventType;
            this.peer = peer;
        }

        /// <summary>
        /// Gets the pad that raised this event.
        /// </summary>
        public GibPad Pad => pad;

        /// <summary>
        /// Gets the type of link event.
        /// </summary>
        public GipPadLinkEventType EventType => eventType;

        /// <summary>
        /// Gets the peer related to this event.
        /// </summary>
        public GibPad? Peer => peer;

    }

}
