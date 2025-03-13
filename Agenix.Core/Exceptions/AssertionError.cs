using System;

namespace Agenix.Core.Exceptions;

public class AssertionError : CoreSystemException
{
    /// <summary>
    ///     Default constructor.
    /// </summary>
    public AssertionError()
    {
    }

    /// <summary>
    ///     Constructor using fields.
    /// </summary>
    /// <param name="message">the string representation of message</param>
    public AssertionError(string message) : base(message)
    {
    }

    /// <summary>
    ///     Constructor using fields.
    /// </summary>
    /// <param name="message">the string representation of message</param>
    /// <param name="cause">The Exception obj.</param>
    public AssertionError(string message, Exception cause) : base(message, cause)
    {
    }
}