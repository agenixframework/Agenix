namespace Agenix.Api.Exceptions;

/// <summary>
///     Exception thrown when a synchronous reply message times out.
/// </summary>
public class ReplyMessageTimeoutException : MessageTimeoutException
{
    /// <summary>
    ///     Exception thrown when a synchronous reply message times out.
    /// </summary>
    public ReplyMessageTimeoutException(long timeout, string endpoint)
        : base(timeout, endpoint)
    {
    }

    /// <summary>
    ///     Exception thrown when a synchronous reply message times out.
    /// </summary>
    public ReplyMessageTimeoutException(long timeout, string endpoint, SystemException cause)
        : base(timeout, endpoint, cause)
    {
    }

    /// <summary>
    ///     Gets an error message describing the reason for the failure to receive a synchronous reply message.
    /// </summary>
    /// <remarks>
    ///     If the Timeout property is less than or equal to zero and the Endpoint property is not set,
    ///     this property returns a default error message.
    ///     Otherwise, it includes the endpoint information in the error message.
    /// </remarks>
    public override string Message
    {
        get
        {
            if (Timeout <= 0 && Endpoint == null) return "Failed to receive synchronous reply message.";

            return $"Failed to receive synchronous reply message on endpoint: '{Endpoint}'";
        }
    }
}