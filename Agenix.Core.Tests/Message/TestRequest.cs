namespace Agenix.Core.Tests.Message;

public class TestRequest
{
    /// <summary>
    ///     Default constructor using message field.
    /// </summary>
    /// <param name="message">The message for the request.</param>
    public TestRequest(string message)
    {
        Message = message;
    }

    /// <summary>
    ///     This request's message.
    /// </summary>
    public string Message { get; set; }
}
