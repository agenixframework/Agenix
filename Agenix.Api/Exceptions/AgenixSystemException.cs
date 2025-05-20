namespace Agenix.Api.Exceptions;

/// <summary>
///     Basic custom runtime/ system exception for all errors in Agenix
/// </summary>
public class AgenixSystemException : SystemException
{
    /// <summary>
    ///     Default constructor.
    /// </summary>
    public AgenixSystemException()
    {
    }

    public AgenixSystemException(string message) : base(message)
    {
    }

    public AgenixSystemException(string message, Exception cause) : base(message, cause)
    {
    }

    public string GetMessage()
    {
        return base.Message;
    }
}