namespace Gip.Core
{

    /// <summary>
    /// Describes the possible state transition states of a <see cref="GipElement"/>.
    /// </summary>
    public enum GipStateChange
    {

        /// <summary>
        /// Element is progressing from <see cref="GipState.Null"/> to <see cref="GipState.Ready"/>.
        /// </summary>
        NullToReady = 10,

        /// <summary>
        /// Element is progressing from <see cref="GipState.Ready"/> to <see cref="GipState.Paused"/>.
        /// </summary>
        ReadyToPaused = 19,

        /// <summary>
        /// Element is progressing from <see cref="GipState.Paused"/> to <see cref="GipState.Running"/>.
        /// </summary>
        PausedToRunning = 28,

        /// <summary>
        /// Element is progressing from <see cref="GipState.Running"/> to <see cref="GipState.Paused"/>.
        /// </summary>
        RunningToPaused = 35,

        /// <summary>
        /// Element is progressing from <see cref="GipState.Paused"/> to <see cref="GipState.Ready"/>.
        /// </summary>
        PausedToReady = 26,

        /// <summary>
        /// Element is progressing from <see cref="GipState.Ready"/> to <see cref="GipState.Null"/>.
        /// </summary>
        ReadyToNull = 17,

        /// <summary>
        /// Element is progressing from <see cref="GipState.Null"/> to <see cref="GipState.Null"/>.
        /// </summary>
        NullToNull = 9,

        /// <summary>
        /// Element is progressing from <see cref="GipState.Ready"/> to <see cref="GipState.Ready"/>.
        /// </summary>
        ReadyToReady = 18,

        /// <summary>
        /// Element is progressing from <see cref="GipState.Paused"/> to <see cref="GipState.Paused"/>.
        /// </summary>
        PausedToPaused = 27,

        /// <summary>
        /// Element is progressing from <see cref="GipState.Running"/> to <see cref="GipState.Running"/>.
        /// </summary>
        RunningToRunning = 36

    }

}
