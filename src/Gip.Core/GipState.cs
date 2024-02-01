namespace Gip.Core
{

    /// <summary>
    /// Describes the possible states of a <see cref="GipElement"/>.
    /// </summary>
    public enum GipState
    {

        /// <summary>
        /// Initialial state of an element.
        /// </summary>
        Null = 0,

        /// <summary>
        /// Element is ready to be run.
        /// </summary>
        Ready = 1,

        /// <summary>
        /// Element has been paused.
        /// </summary>
        Paused = 2,

        /// <summary>
        /// Element is running.
        /// </summary>
        Running = 3,

    }

}
