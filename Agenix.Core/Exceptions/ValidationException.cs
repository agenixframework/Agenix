using System;

namespace Agenix.Core.Exceptions
{
    public class ValidationException : CoreSystemException
    {
        /// <summary>
        ///     Default constructor.
        /// </summary>
        public ValidationException()
        {
        }

        /// <summary>
        ///     Constructor using fields.
        /// </summary>
        /// <param name="message">the string representation of message</param>
        public ValidationException(string message) : base(message)
        {
        }

        /// <summary>
        ///     Constructor using fields.
        /// </summary>
        /// <param name="message">the string representation of message</param>
        /// <param name="cause">The Exception obj.</param>
        public ValidationException(string message, Exception cause) : base(message, cause)
        {
        }
    }
}