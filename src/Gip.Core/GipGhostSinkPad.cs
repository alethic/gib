namespace Gip.Core
{

    public sealed class GipGhostSinkPad : GipSinkPad
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="template"></param>
        /// <param name="name"></param>
        /// <param name="caps"></param>
        internal GipGhostSinkPad(GipElement element, GipSinkPadTemplate template, string name, GipCap[] caps) :
            base(element, template, name, caps)
        {

        }

    }

}
