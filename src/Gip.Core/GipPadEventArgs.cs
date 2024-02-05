namespace Gip.Core
{

    /// <summary>
    /// Arguments passed to events of a <see cref="GibPad"/>.
    /// </summary>
    public abstract class GipPadEventArgs : GipEventArgs
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="target"></param>
        protected GipPadEventArgs(GipPad target) : 
            base(target)
        {

        }

    }

}
