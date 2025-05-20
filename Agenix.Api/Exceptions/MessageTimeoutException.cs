namespace Agenix.Api.Exceptions;

/// <summary>
///     Exception thrown when a message times out.
/// </summary>
public class MessageTimeoutException : ActionTimeoutException
{
    protected readonly string Endpoint;

    /// <summary>
    ///     Default constructor
    /// </summary>
    public MessageTimeoutException()
        : this(0L, "")
    {
    }

    /// <summary>
    ///     Default constructor
    /// </summary>
    public MessageTimeoutException(long timeout, string endpoint)
        : base(timeout)
    {
        Endpoint = endpoint;
    }

    /// <summary>
    ///     Exception thrown when a message times out.
    /// </summary>
    public MessageTimeoutException(long timeout, string endpoint, SystemException cause)
        : base(timeout, cause)
    {
        Endpoint = endpoint;
    }

    /// <summary>
    ///     Overrides the detail message
    /// </summary>
    /// <returns>A detailed message as a string.</returns>
    protected override string GetDetailMessage()
    {
        if (Timeout <= 0 && Endpoint == null) return "Failed to receive message.";

        return $"Failed to receive message on endpoint: '{Endpoint}'";
    }
}