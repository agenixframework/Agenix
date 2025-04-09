#region Imports

using System;
using System.Runtime.Serialization;

#endregion

namespace Agenix.Core.Exceptions
{
    /// <summary>
    /// Superclass for all exceptions thrown in the Objects namespace and sub-namespaces.
    /// </summary>
    [Serializable]
    public class ReflectionException : ApplicationException
    {
        #region Constructor (s) / Destructor
        /// <summary>Creates a new instance of the ObjectsException class.</summary>
        public ReflectionException()
        {
        }

        /// <summary>
        /// Creates a new instance of the ObjectsException class. with the specified message.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        public ReflectionException (string message) : base(message)
        {
        }

        /// <summary>
        /// Creates a new instance of the ObjectsException class with the specified message
        /// and root cause.
        /// </summary>
        /// <param name="message">
        /// A message about the exception.
        /// </param>
        /// <param name="rootCause">
        /// The root exception that is being wrapped.
        /// </param>
        public ReflectionException (string message, Exception rootCause)
            : base(message, rootCause)
        {
        }

        /// <summary>
        /// Creates a new instance of the ObjectsException class.
        /// </summary>
        /// <param name="info">
        /// The <see cref="System.Runtime.Serialization.SerializationInfo"/>
        /// that holds the serialized object data about the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The <see cref="System.Runtime.Serialization.StreamingContext"/>
        /// that contains contextual information about the source or destination.
        /// </param>
        protected ReflectionException (
            SerializationInfo info, StreamingContext context)
            : base (info, context)
        {
        }
        #endregion
    }
}
