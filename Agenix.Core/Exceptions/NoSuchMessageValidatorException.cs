using System;

namespace Agenix.Core.Exceptions;

/// <summary>
///     In case no message validator exists for a given prefix this exception is thrown.
/// </summary>
public class NoSuchMessageValidatorException : CoreSystemException
{
    /// <summary>
    ///     Thrown when no message validator exists for a given prefix.
    /// </summary>
    public NoSuchMessageValidatorException()
    {
    }

    /// <summary>
    ///     Thrown when no message validator exists for a given prefix.
    /// </summary>
    public NoSuchMessageValidatorException(string message) : base(message)
    {
    }


    /// <summary>
    ///     In case no message validator exists for a given prefix this exception is thrown.
    /// </summary>
    public NoSuchMessageValidatorException(string message, Exception cause) : base(message, cause)
    {
    }
}