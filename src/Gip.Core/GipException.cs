using System;

namespace Gip.Core
{

    public class GipException : Exception
    {

        /// <inheritdoc />
        public GipException()
        {

        }

        /// <inheritdoc />
        public GipException(string? message) : base(message)
        {
        }

        /// <inheritdoc />
        public GipException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

    }

}