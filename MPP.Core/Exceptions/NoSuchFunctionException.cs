using System;

namespace MPP.Core.Exceptions
{
    /// <summary>
    ///     Unknown functions cause this exception.
    /// </summary>
    public class NoSuchFunctionException : CoreSystemException
    {
        /// <summary>
        ///     Default constructor.
        /// </summary>
        public NoSuchFunctionException()
        {
        }

        /// <summary>
        ///     Constructor using fields.
        /// </summary>
        /// <param name="message">the string representation of message</param>
        public NoSuchFunctionException(string message) : base(message)
        {
        }

        /// <summary>
        ///     Constructor using fields.
        /// </summary>
        /// <param name="message">the string representation of message</param>
        /// <param name="cause">The Exception obj.</param>
        public NoSuchFunctionException(string message, Exception cause) : base(message, cause)
        {
        }
    }
}