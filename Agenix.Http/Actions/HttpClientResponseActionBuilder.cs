using System.Net;
using Agenix.Api.Message;
using Agenix.Api.Util;
using Agenix.Core.Actions;
using Agenix.Core.Message;
using Agenix.Core.Message.Builder;
using Agenix.Core.Util;
using Agenix.Http.Message;

namespace Agenix.Http.Actions;

/// Facilitates the creation and configuration of HTTP client response actions.
/// This class extends the capabilities of ReceiveMessageAction.Builder to provide specialized support
/// for constructing HTTP response messages. It allows users to define message properties such as headers,
/// payload, and metadata, which are tailored for handling HTTP interactions.
public class HttpClientResponseActionBuilder : ReceiveMessageAction.ReceiveMessageActionBuilder<ReceiveMessageAction, 
    HttpClientResponseActionBuilder.HttpMessageBuilderSupport, HttpClientResponseActionBuilder>
{
    private readonly HttpMessage _httpMessage;

    public  HttpClientResponseActionBuilder()
    {
        _httpMessage = new HttpMessage();
        Message(new HttpMessageBuilder(_httpMessage)).HeaderIgnoreCase = true;
    }

    public HttpClientResponseActionBuilder(IMessageBuilder messageBuilder, HttpMessage httpMessage)
    {
        _httpMessage = httpMessage;
        Message(messageBuilder).HeaderIgnoreCase = true;
    }

    /// Retrieves the HTTP message builder support instance for customizing the construction of HTTP messages.
    /// This method creates a new `HttpMessageBuilderSupport` instance if it has not been initialized.
    /// It ensures that the base implementation is invoked and the proper type is returned.
    /// <return>The `HttpMessageBuilderSupport` instance associated with this builder.</return>
    public override HttpMessageBuilderSupport GetMessageBuilderSupport()
    {
        messageBuilderSupport ??= new HttpMessageBuilderSupport(_httpMessage, this);

        return base.GetMessageBuilderSupport();
    }

    /// Retrieves the message payload as an optional string.
    /// Returns the payload from the associated HttpMessage if it exists and is a string.
    /// If no valid payload is found, retrieves the payload from the base implementation.
    /// <return>An optional string containing the message payload or empty if no payload is present.</return>
    protected override Optional<string> GetMessagePayload()
    {
        return _httpMessage.Payload is string
            ? Optional<string>.Of(_httpMessage.GetPayload<string>())
            : base.GetMessagePayload();
    }

    /*public ReceiveMessageAction(
        ReceiveMessageActionBuilder<ReceiveMessageAction, ReceiveMessageActionBuilderSupport, Builder> builder)
        : base(builder.GetName() ?? "receive", builder.GetDescription() ?? "")
    {
        Endpoint = builder.GetEndpoint();
        EndpointUri = builder.GetEndpointUri();
        ReceiveTimeout = builder._receiveTimeout;
        Selector = builder._messageSelector;
        MessageSelectors = builder._messageSelectors;
        Validators = builder._validators;
        Processor = builder._validationProcessor;
        ValidationContexts = builder.GetValidationContexts();
        VariableExtractors = builder.GetVariableExtractors();
        Processors = builder.GetMessageProcessors();
        MessageBuilder = builder.GetMessageBuilderSupport().GetMessageBuilder();
        ControlMessageProcessors = builder.GetMessageBuilderSupport().ControlMessageProcessors;
        MessageType = builder.GetMessageBuilderSupport().GetMessageType();
    }*/
    
    protected override ReceiveMessageAction DoBuild()
    {
        var builder = new ReceiveMessageAction.Builder();
        builder.Name(GetName());
        builder.Description(GetDescription());
        builder.Endpoint(GetEndpoint());
        builder.Endpoint(GetEndpointUri());
        builder.Timeout(_receiveTimeout);
        builder.Selector(_messageSelector);
        builder.Selector(_messageSelectors);
        builder.Validators(_validators);
        builder.Validate(ValidationContexts);
        
        if (_validationProcessor != null)
        {
            builder.Process(_validationProcessor);
        }
        
        foreach (var extractor in GetVariableExtractors())
        {
            builder.Process(extractor);
        }
        
        foreach (var processor in GetMessageProcessors())
        {
            builder.Process(processor);
        }

        builder.GetMessageBuilderSupport().From(GetMessageBuilderSupport().GetMessageBuilder());
        builder.GetMessageBuilderSupport().Type(GetMessageBuilderSupport().GetMessageType());
        
        foreach (var controlMessageProcessor in GetMessageBuilderSupport().ControlMessageProcessors)
        {
            builder.GetMessageBuilderSupport().ControlMessageProcessors.Add(controlMessageProcessor);
        }

        return new ReceiveMessageAction(builder);
    }

    /// Provides support for building HTTP messages within the context of the ReceiveMessageAction.
    /// This class serves as a helper for configuring various properties of an HTTP message, including
    /// the name, payload, status, and associated metadata required for HTTP message handling.
    public class HttpMessageBuilderSupport(HttpMessage httpMessage, HttpClientResponseActionBuilder dlg) : ReceiveMessageBuilderSupport<ReceiveMessageAction, HttpClientResponseActionBuilder, HttpMessageBuilderSupport>(dlg)
    {
        private readonly HttpMessage httpMessage = httpMessage;
        
        
        /// Sets the name of the HTTP message and returns the current builder instance for method chaining.
        /// This method updates the `Name` property of the associated HTTP message and invokes the base implementation.
        /// <param name="name">The name to be assigned to the HTTP message.</param>
        /// <return>The current builder instance, allowing for method chaining.</return>
        public override HttpMessageBuilderSupport Name(string name)
        {
            httpMessage.Name = name;
            return base.Name(name);
        }

        /// Updates the body (payload) of the HTTP message and returns the current builder instance for method chaining.
        /// This method sets the `Payload` property of the associated `HttpMessage`.
        /// <param name="payload">The payload to be assigned to the HTTP message body.</param>
        /// <return>The current builder instance, allowing for method chaining.</return>
        public override HttpMessageBuilderSupport Body(string payload)
        {
            httpMessage.Payload = payload;
            return this;
        }

        /// Copies the properties of the specified `IMessage` instance to the internal `HttpMessage` instance.
        /// This method utilizes the `HttpMessageUtils.Copy` utility to transfer data from the source message.
        /// <param name="controlMessage">The source message from which properties will be copied.</param>
        /// <return>The current `HttpMessageBuilderSupport` instance, allowing for method chaining.</return>
        public override HttpMessageBuilderSupport From(IMessage controlMessage)
        {
            HttpMessageUtils.Copy(controlMessage, httpMessage);
            return this;
        }

        /// Sets the HTTP status code for the HTTP message and returns the current builder instance for method chaining.
        /// This method updates the `Status` property of the associated HTTP message and invokes the `Status` method of the `HttpMessage` instance.
        /// <param name="httpStatusCode">The HTTP status code to be assigned to the message.</param>
        /// <return>The current builder instance, allowing for method chaining.</return>
        public HttpMessageBuilderSupport Status(HttpStatusCode httpStatusCode)
        {
            httpMessage.Status(httpStatusCode);
            return this;
        }

        /// Assigns the provided HTTP status code as an integer to the message and updates the corresponding HTTP headers.
        /// This method sets the status code by converting it to the `HttpStatusCode` type and modifies the message accordingly.
        /// <param name="statusCode">The integer value of the status code to be assigned to the HTTP message.</param>
        /// <return>The current builder instance, enabling method chaining for further configuration.</return>
        public HttpMessageBuilderSupport StatusCode(int statusCode)
        {
            httpMessage.Status((HttpStatusCode)statusCode);
            return this;
        }

        /// Sets the HTTP response reason phrase and returns the current builder instance for method chaining.
        /// This method updates the `ReasonPhrase` property of the associated HTTP message.
        /// <param name="reasonPhrase">The reason phrase to be assigned to the HTTP message.</param>
        /// <return>The current builder instance, enabling continued method chaining.</return>
        public HttpMessageBuilderSupport ReasonPhrase(string reasonPhrase)
        {
            httpMessage.ReasonPhrase(reasonPhrase);
            return this;
        }

        /// Sets the HTTP version of the message and returns the current builder instance for method chaining.
        /// This method updates the `Version` property of the associated HTTP message and invokes the base implementation.
        /// <param name="version">The HTTP version to be assigned to the message.</param>
        /// <return>The current builder instance, allowing for method chaining.</return>
        public HttpMessageBuilderSupport Version(string version)
        {
            httpMessage.Version(version);
            return this;
        }

        /// Sets the content type of the HTTP message and returns the current builder instance for method chaining.
        /// This method updates the `ContentType` property of the associated HTTP message.
        /// <param name="contentType">The content type value to be assigned to the HTTP message.</param>
        /// <return>The current builder instance, allowing for method chaining.</return>
        public HttpMessageBuilderSupport ContentType(string contentType)
        {
            httpMessage.ContentType(contentType);
            return this;
        }

        /// Adds a cookie to the HTTP message and returns the current builder instance for method chaining.
        /// This method adds or updates the cookie in the associated HTTP message and modifies the headers accordingly.
        /// <param name="cookie">The Cookie object to be added or updated in the HTTP message.</param>
        /// <return>The current builder instance, enabling method chaining.</return>
        public HttpMessageBuilderSupport Cookie(Cookie cookie)
        {
            httpMessage.Cookie(cookie);
            return this;
        }
    }
}