#region Imports

using System;
using System.Runtime.Serialization;

#endregion

namespace Agenix.Core.Exceptions
{
    /// <summary>
    /// Thrown on an unrecoverable problem encountered in the
    /// objects namespace or sub-namespaces, e.g. bad class or field.
    /// </summary>
    [Serializable]
    public class FatalReflectionException : ReflectionException
    {
        /// <summary>
        /// Creates a new instance of the FatalObjectException class.
        /// </summary>
        public FatalReflectionException()
        {
        }

        /// <summary>
        /// Creates a new instance of the FatalObjectException class with the
        /// specified message.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        public FatalReflectionException(string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the FatalObjectException class with the
        /// specified message.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="rootCause">
        /// The root exception that is being wrapped.
        /// </param>
        public FatalReflectionException(string message, Exception rootCause)
            : base(message, rootCause)
        {
        }

        /// <summary>
        /// Creates a new instance of the FatalObjectException class.
        /// </summary>
        /// <param name="info">
        /// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
        /// that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="System.Runtime.Serialization.StreamingContext"/>
        /// that contains contextual information about the source or destination.
        /// </param>
        protected FatalReflectionException (
            SerializationInfo info, StreamingContext context)
            : base (info, context)
        {
        }
    }
}
