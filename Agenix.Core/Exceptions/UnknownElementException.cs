using System;

namespace Agenix.Core.Exceptions
{
    public class UnknownElementException : CoreSystemException
    {
        /// <summary>
        ///     Default constructor.
        /// </summary>
        public UnknownElementException()
        {
        }

        /// <summary>
        ///     Constructor using fields.
        /// </summary>
        /// <param name="message">the string representation of message</param>
        public UnknownElementException(string message) : base(message)
        {
        }

        /// <summary>
        ///     Constructor using fields.
        /// </summary>
        /// <param name="message">the string representation of message</param>
        /// <param name="cause">The Exception obj.</param>
        public UnknownElementException(string message, Exception cause) : base(message, cause)
        {
        }
    }
}