using System;

namespace MPP.Core.Exceptions
{
    /// <summary>
    ///     In case no function library exists for a given prefix this exception is thrown.
    /// </summary>
    public class NoSuchFunctionLibraryException : CoreSystemException
    {
        /// <summary>
        ///     Default constructor.
        /// </summary>
        public NoSuchFunctionLibraryException()
        {
        }

        /// <summary>
        ///     Constructor using fields.
        /// </summary>
        /// <param name="message">the string representation of message</param>
        public NoSuchFunctionLibraryException(string message) : base(message)
        {
        }

        /// <summary>
        ///     Constructor using fields.
        /// </summary>
        /// <param name="message">the string representation of message</param>
        /// <param name="cause">The Exception obj.</param>
        public NoSuchFunctionLibraryException(string message, Exception cause) : base(message, cause)
        {
        }
    }
}