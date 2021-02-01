using System;

namespace MPP.Core.Exceptions
{
    /// <summary>
    /// Basic custom runtime/ system exception for all errors in Core
    /// </summary>
    public class CoreSystemException : SystemException
    {
        /// <summary>
        /// Default constructor.
        /// </summary>
        public CoreSystemException()
        {

        }

        public CoreSystemException(string message) : base(message)
        {
            
        }

        public CoreSystemException(string message, Exception cause) : base(message, cause)
        {

        }

        public string GetMessage()
        {
            return base.Message;
        }
    }
}
