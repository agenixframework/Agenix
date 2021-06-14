using System;

namespace FleetPay.Core.Exceptions
{
    /// <summary>
    ///     Use this exception in case a function is called with invalid parameters.
    /// </summary>
    public class InvalidFunctionUsageException : CoreSystemException
    {
        /// <summary>
        ///     Default constructor.
        /// </summary>
        public InvalidFunctionUsageException()
        {
        }

        /// <summary>
        ///     Constructor using fields.
        /// </summary>
        /// <param name="message">the string representation of message</param>
        public InvalidFunctionUsageException(string message) : base(message)
        {
        }

        /// <summary>
        ///     Constructor using fields.
        /// </summary>
        /// <param name="message">the string representation of message</param>
        /// <param name="cause">The Exception obj.</param>
        public InvalidFunctionUsageException(string message, Exception cause) : base(message, cause)
        {
        }
    }
}