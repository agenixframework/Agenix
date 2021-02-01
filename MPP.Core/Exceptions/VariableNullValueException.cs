using System;

namespace MPP.Core.Exceptions
{
    public class VariableNullValueException : CoreSystemException
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public VariableNullValueException() : base()
        {
        }

        /// <summary>
        /// Constructor using fields.
        /// </summary>
        /// <param name="message">the string representation of message</param>
        public VariableNullValueException(string message) : base(message)
        {
        }

        /// <summary>
        /// Constructor using fields.
        /// </summary>
        /// <param name="message">the string representation of message</param>
        /// <param name="cause">The Exception obj.</param>
        public VariableNullValueException(string message, Exception cause) : base(message, cause)
        {
        }
    }
}