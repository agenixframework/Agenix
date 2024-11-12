using System;

namespace Agenix.Core.Exceptions;

/// <summary>
///     This exception is thrown when a specified path is not found.
/// </summary>
public class PathNoFoundException : CoreSystemException
{
    /// <summary>
    ///     Default constructor.
    /// </summary>
    public PathNoFoundException()
    {
    }

    /// <summary>
    ///     Constructor using fields.
    /// </summary>
    /// <param name="message">the string representation of message</param>
    public PathNoFoundException(string message) : base(message)
    {
    }

    /// <summary>
    ///     Constructor using fields.
    /// </summary>
    /// <param name="message">the string representation of message</param>
    /// <param name="cause">The Exception obj.</param>
    public PathNoFoundException(string message, Exception cause) : base(message, cause)
    {
    }
}