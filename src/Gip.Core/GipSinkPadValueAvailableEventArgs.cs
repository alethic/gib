using System;

namespace Gip.Core
{

    public class GipSinkPadValueAvailableEventArgs :
        EventArgs
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="value"></param>
        internal GipSinkPadValueAvailableEventArgs(object? value)
        {
            Value = value;
        }

        /// <summary>
        /// Gets the current value.
        /// </summary>
        public object? Value { get; }

    }

}
