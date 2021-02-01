using System;

namespace MPP.Core.Exceptions
{
    /// <summary>
    /// Throw this exception in case an unknown variable is read from test context.
    /// </summary>
    public class NoSuchVariableException : CoreSystemException
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public NoSuchVariableException() : base()
        {
        }

        /// <summary>
        /// Constructor using fields.
        /// </summary>
        /// <param name="message">the string representation of message</param>
        public NoSuchVariableException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructor using fields.
        /// </summary>
        /// <param name="message">the string representation of message</param>
        /// <param name="cause">The Exception obj.</param>
        public NoSuchVariableException(string message, Exception cause) : base(message, cause)
        {
        }
    }
}