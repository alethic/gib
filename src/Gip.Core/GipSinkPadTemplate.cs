namespace Gip.Core
{

    /// <summary>
    /// <see cref="GipSendPadTemplate"/>s describes the availability of send pads for an element type.
    /// </summary>
    public sealed class GipSinkPadTemplate : GipPadTemplate
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="presence"></param>
        /// <param name="caps"></param>
        public GipSinkPadTemplate(string name, GipPadPresence presence, GipCapList caps) :
            base(name, presence, caps)
        {

        }

        /// <summary>
        /// Creates a new <see cref="GipSinkPad"/> from this template.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public GipSinkPad Create()
        {
            return new GipSinkPad(this);
        }

    }

}
