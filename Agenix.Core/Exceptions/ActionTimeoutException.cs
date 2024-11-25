using System;

namespace Agenix.Core.Exceptions;

/// <summary>
///     Represents an exception that is thrown when an action times out.
/// </summary>
public class ActionTimeoutException : CoreSystemException
{
    protected readonly long Timeout;

    /// <summary>
    ///     Default constructor
    /// </summary>
    public ActionTimeoutException()
        : this(0L)
    {
    }

    /// <summary>
    ///     Default constructor
    /// </summary>
    public ActionTimeoutException(long timeout)
    {
        Timeout = timeout;
    }

    /// <summary>
    ///     Default constructor
    /// </summary>
    public ActionTimeoutException(long timeout, SystemException cause)
        : base(cause.Message)
    {
        Timeout = timeout;
    }

    /// <summary>
    ///     Overrides the Message property to provide a custom timeout message, indicating the duration of the timeout, if
    ///     specified.
    /// </summary>
    public override string Message => Timeout <= 0
        ? $"Action timeout. {GetDetailMessage()}".Trim()
        : $"Action timeout after {Timeout} milliseconds. {GetDetailMessage()}".Trim();

    /// <summary>
    ///     Provides a detailed message which can be overridden.
    /// </summary>
    /// <returns>A detailed message as a string.</returns>
    protected virtual string GetDetailMessage()
    {
        return string.Empty;
    }
}