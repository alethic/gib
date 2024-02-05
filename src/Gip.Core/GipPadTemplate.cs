using System;

namespace Gip.Core
{

    public abstract class GipPadTemplate
    {

        readonly string name;
        readonly GipPadPresence presence;
        readonly GipCapList caps;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="presence"></param>
        /// <param name="caps"></param>
        public GipPadTemplate(string name, GipPadPresence presence, GipCapList caps)
        {
            this.name = name ?? throw new ArgumentNullException(nameof(name));
            this.presence = presence;
            this.caps = caps ?? GipCapList.Empty;
        }

        /// <summary>
        /// Gets the name template that determines the name of allocated pads.
        /// </summary>
        public string Name => name;

        /// <summary>
        /// Gets the presence type of the template.
        /// </summary>
        public GipPadPresence Presence => presence;

        /// <summary>
        /// Gets the caps of the template.
        /// </summary>
        public GipCapList Caps => caps;

    }

}