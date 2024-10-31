using System;

namespace Agenix.Core.Exceptions;

public class NoSuchValidationMatcherException : CoreSystemException
{
    /// <summary>
    ///     Default constructor.
    /// </summary>
    public NoSuchValidationMatcherException()
    {
    }

    /// <summary>
    ///     Constructor using fields.
    /// </summary>
    /// <param name="message">the string representation of message</param>
    public NoSuchValidationMatcherException(string message) : base(message)
    {
    }

    /// <summary>
    ///     Constructor using fields.
    /// </summary>
    /// <param name="message">the string representation of message</param>
    /// <param name="cause">The Exception obj.</param>
    public NoSuchValidationMatcherException(string message, Exception cause) : base(message, cause)
    {
    }
}