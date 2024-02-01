namespace Gip.Core
{

    public class GipElementStateChangedEventArgs : GipElementEventArgs
    {

        readonly GipState oldState;
        readonly GipState newState;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="element"></param>
        /// <param name="oldState"></param>
        /// <param name="newState"></param>
        public GipElementStateChangedEventArgs(GipElement element, GipState oldState, GipState newState) :
            base(element)
        {
            this.oldState = oldState;
            this.newState = newState;
        }

        /// <summary>
        /// Gets the old state of the element.
        /// </summary>
        public GipState OldState => oldState;

        /// <summary>
        /// Gets the new state of the element.
        /// </summary>
        public GipState NewState => newState;

    }

}
