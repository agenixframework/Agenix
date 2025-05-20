using System.Net;
using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Message;
using Agenix.Core.Message.Builder;

namespace Agenix.Http.Message;

/// The HttpMessageBuilder class facilitates the construction of HTTP messages with specified configurations, ensuring unique message identifiers and appropriate timestamps. It extends the StaticMessageBuilder, providing additional functionality tailored to HTTP message creation, such as handling cookies and setting message-specific details.
/// /
public class HttpMessageBuilder : StaticMessageBuilder
{
    private readonly CookieEnricher _cookieEnricher;

    /// The HttpMessageBuilder class is responsible for creating HTTP messages by extending the functionality of StaticMessageBuilder. It provides additional HTTP-specific features such as handling cookies and setting message-related details unique to HTTP communication. This class ensures that each message is assigned a unique identifier and that all timestamps are correctly configured.
    public HttpMessageBuilder(HttpMessage message) : this(message, new CookieEnricher())
    {
    }

    /// The HttpMessageBuilder class facilitates the construction of HTTP messages with specified configurations, ensuring unique message identifiers and appropriate timestamps. It extends the StaticMessageBuilder, providing additional functionality tailored to HTTP message creation, such as handling cookies and setting message-specific details.
    public HttpMessageBuilder(HttpMessage message,
        CookieEnricher cookieEnricher) : base(message)
    {
        _cookieEnricher = cookieEnricher;
    }

    /// Constructs and returns an HTTP message using the specified test execution context and message type.
    /// This method extends the functionality of the base message building process by creating an HTTP-specific message,
    /// ensuring that additional configurations such as cookies and headers are handled appropriately.
    /// <param name="context">
    ///     The test execution context used to construct the message and manage its state within a test
    ///     scenario.
    /// </param>
    /// <param name="messageType">
    ///     The type identifier for the message being constructed, influencing its structure and
    ///     behavior.
    /// </param>
    /// <return>
    ///     An IMessage instance representing the newly constructed HTTP message, complete with headers, cookies, and other
    ///     HTTP-specific attributes.
    /// </return>
    public override IMessage Build(TestContext context, string messageType)
    {
        //Copy the initial message, so that it is not manipulated during the test.
        var message = new HttpMessage(base.GetMessage(), AgenixSettings.IsHttpMessageBuilderForceHeaderUpdateEnabled());

        var constructed = base.Build(context, messageType);

        message.Name = constructed.Name;
        message.SetType(constructed.GetType());
        message.Payload = constructed.Payload;
        message.SetCookies(ConstructCookies(context));
        ReplaceHeaders(constructed, message);

        return message;
    }

    /// Replaces the headers in the target message with headers from the source message, excluding filtered headers.
    /// <param name="from">The source message from which to retrieve headers.</param>
    /// <param name="to">The target message to which the headers should be set.</param>
    private void ReplaceHeaders(IMessage from, IMessage to)
    {
        var headerKeys = new HashSet<string>(to.GetHeaders().Keys)
            .Where(key => !FilteredHeaders.Contains(key))
            .ToHashSet();

        foreach (var key in headerKeys) to.GetHeaders().Remove(key);

        foreach (var entry in from.GetHeaders().Where(entry => !FilteredHeaders.Contains(entry.Key)))
            to.GetHeaders()[entry.Key] = entry.Value;
    }

    /// Replaces the dynamic content within the cookies of an HTTP message based on the provided context.
    /// <param name="context">
    ///     The context containing variables and parameters used to enrich and replace dynamic content in the
    ///     cookies.
    /// </param>
    /// <return>A collection of enriched cookies where variables have been replaced according to the provided context.</return>
    private Cookie[] ConstructCookies(TestContext context)
    {
        var cookies = _cookieEnricher.Enrich(GetMessage().GetCookies(), context);
        return cookies.ToArray();
    }

    public override HttpMessage GetMessage()
    {
        return (HttpMessage)base.GetMessage();
    }
}