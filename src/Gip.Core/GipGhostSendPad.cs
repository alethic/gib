namespace Gip.Core
{

    public sealed class GipGhostSendPad : GipSendPad
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="template"></param>
        /// <param name="name"></param>
        /// <param name="caps"></param>
        internal GipGhostSendPad(GipElement element, GipSendPadTemplate template, string name, GipCap[] caps) :
            base(element, template, name, caps)
        {

        }

    }

}
