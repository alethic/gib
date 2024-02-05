namespace Gip.Core
{

    public abstract class GipElementEventArgs : GipEventArgs
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="element"></param>
        public GipElementEventArgs(GipElement element) :
            base(element)
        {

        }

        /// <summary>
        /// Gets the element that raised this event.
        /// </summary>
        public new GipElement Target => (GipElement)base.Target;

    }

}
