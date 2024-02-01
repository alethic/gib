namespace Gip.Core
{

    /// <summary>
    /// <see cref="GipSendPadTemplate"/>s describes the availability of send pads for an element type.
    /// </summary>
    public sealed class GipSendPadTemplate : GipPadTemplate
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="presence"></param>
        /// <param name="caps"></param>
        public GipSendPadTemplate(string name, GipPadPresence presence, GipCap[] caps) : 
            base(name, presence, caps)
        {

        }

        /// <summary>
        /// Creates a new <see cref="GipSendPad"/> from this template.
        /// </summary>
        /// <returns></returns>
        public GipSendPad Create()
        {
            return new GipSendPad(this);
        }

    }

}
