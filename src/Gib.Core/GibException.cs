using System;

namespace Gib.Core
{

    public class GibException : Exception
    {

        /// <inheritdoc />
        public GibException()
        {

        }

        /// <inheritdoc />
        public GibException(string? message) : base(message)
        {
        }

        /// <inheritdoc />
        public GibException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

    }

}