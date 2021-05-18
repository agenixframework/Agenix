using System;

namespace MPP.Core.Exceptions
{
    /// <summary>
    ///     In case no function library exists for a given prefix this exception is thrown.
    /// </summary>
    public class NoSuchValidationMatcherLibraryException : CoreSystemException
    {
        /// <summary>
        ///     Default constructor.
        /// </summary>
        public NoSuchValidationMatcherLibraryException()
        {
        }

        /// <summary>
        ///     Constructor using fields.
        /// </summary>
        /// <param name="message">the string representation of message</param>
        public NoSuchValidationMatcherLibraryException(string message) : base(message)
        {
        }

        /// <summary>
        ///     Constructor using fields.
        /// </summary>
        /// <param name="message">the string representation of message</param>
        /// <param name="cause">The Exception obj.</param>
        public NoSuchValidationMatcherLibraryException(string message, Exception cause) : base(message, cause)
        {
        }
    }
}