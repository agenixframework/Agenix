using System.Net;
using Agenix.Api.Message;
using Agenix.Core.Actions;
using Agenix.Core.Message;
using Agenix.Core.Message.Builder;
using Agenix.Http.Message;

namespace Agenix.Http.Actions;

/// Provides functionality for building an HTTP client request with customizable
/// attributes such as HTTP method, URI, path, query parameters, and more.
/// This builder extends the capabilities of `SendMessageAction.Builder`
/// by adding HTTP-specific configurations and behaviors.
public class HttpClientRequestActionBuilder :SendMessageAction.SendMessageActionBuilder<SendMessageAction, HttpClientRequestActionBuilder.HttpMessageBuilderSupport, HttpClientRequestActionBuilder>
{
    private readonly HttpMessage _httpMessage;

    public HttpClientRequestActionBuilder()
    {
        _httpMessage = new HttpMessage();
        Message(new HttpMessageBuilder(_httpMessage));
    }

    public HttpClientRequestActionBuilder(IMessageBuilder messageBuilder, HttpMessage httpMessage)
    {
        _httpMessage = httpMessage;
        Message(messageBuilder);
    }

    /// Retrieves the instance of the HttpMessageBuilderSupport associated with the current builder.
    /// <return>The instance of HttpMessageBuilderSupport initialized for this request.</return>
    public override HttpMessageBuilderSupport GetMessageBuilderSupport()
    {
        messageBuilderSupport ??= new HttpMessageBuilderSupport(_httpMessage, this);

        return base.GetMessageBuilderSupport();
    }

    /// Sets the request path dynamically for the HTTP message in the builder.
    /// <param name="path">The part of the path to add to the request URI.</param>
    /// <return>The updated instance of HttpClientRequestActionBuilder.</return>
    public HttpClientRequestActionBuilder Path(string path)
    {
        _httpMessage.Path(path);
        return this;
    }

    /// Sets the HTTP request method for the HTTP message in the builder.
    /// <param name="method">The HTTP method to set as the request method.</param>
    /// <return>The updated instance of HttpClientRequestActionBuilder.</return>
    public HttpClientRequestActionBuilder Method(HttpMethod method)
    {
        _httpMessage.Method(method);
        return this;
    }

    /// Sets the request URI dynamically for the HTTP message in the builder.
    /// <param name="uri">The URI to set for the HTTP request.</param>
    /// <return>The updated instance of HttpClientRequestActionBuilder.</return>
    public HttpClientRequestActionBuilder Uri(string uri)
    {
        _httpMessage.Uri(uri);
        return this;
    }

    /// Adds a query parameter to the HTTP message.
    /// <param name="name">The name of the query parameter to add. It must not be null or empty.</param>
    /// <return>The updated instance of HttpClientRequestActionBuilder.</return>
    public HttpClientRequestActionBuilder QueryParam(string name)
    {
        _httpMessage.QueryParam(name);
        return this;
    }

    /// Adds a query parameter to the HTTP request dynamically in the builder.
    /// <param name="name">The name of the query parameter to add. Must not be null or empty.</param>
    /// <param name="value">The value of the query parameter to add. Can be null if no value is specified.</param>
    /// <return>The updated instance of HttpClientRequestActionBuilder.</return>
    public HttpClientRequestActionBuilder QueryParam(string name, string value)
    {
        _httpMessage.QueryParam(name, value);
        return this;
    }
    
    protected override SendMessageAction DoBuild()
    {
        var builder = new SendMessageAction.Builder();
        builder.Name(GetName());
        builder.Description(GetDescription());
        builder.Fork(ForkMode);
        builder.Endpoint(GetEndpoint());
        builder.Endpoint(GetEndpointUri());
        
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
        
        return new SendMessageAction(builder);
    }

    /// Provides support for building an HTTP message with configurable properties
    /// such as payload, method, URI, headers, and cookies.
    /// This class extends the capabilities of `SendMessageActionBuilderSupport`
    /// to include HTTP-specific configurations.
    public class HttpMessageBuilderSupport(HttpMessage httpMessage, HttpClientRequestActionBuilder newDelegate)
        : SendMessageBuilderSupport<SendMessageAction, HttpClientRequestActionBuilder, HttpMessageBuilderSupport>(newDelegate)
    {
        private readonly HttpMessage httpMessage = httpMessage;
        
        /// Adds a payload to the HTTP message being built.
        /// <param name="payload">The content to set as the payload of the HTTP message.</param>
        /// <return>The updated instance of HttpMessageBuilderSupport.</return>
        public override HttpMessageBuilderSupport Body(string payload)
        {
            httpMessage.Payload = payload;
            return this;
        }

        /// Sets the name for the HTTP message in the builder.
        /// <param name="name">The name to assign to the HTTP message.</param>
        /// <return>The updated instance of HttpMessageBuilderSupport.</return>
        public override HttpMessageBuilderSupport Name(string name)
        {
            httpMessage.Name = name;
            return base.Name(name);
        }

        /// Copies the details from the provided control message to the current HTTP message instance.
        /// <param name="controlMessage">The control message from which to copy information.</param>
        /// <return>The updated instance of HttpMessageBuilderSupport.</return>
        public override HttpMessageBuilderSupport From(IMessage controlMessage)
        {
            HttpMessageUtils.Copy(controlMessage, httpMessage);
            return this;
        }

        /// Sets the HTTP method for the HTTP message being built.
        /// <param name="method">The HTTP method to apply to the HTTP message.</param>
        /// <return>The updated instance of HttpMessageBuilderSupport.</return>
        public HttpMessageBuilderSupport Method(HttpMethod method)
        {
            httpMessage.Method(method);
            return this;
        }

        /// Configures the URI of the HTTP message being built.
        /// <param name="uri">The URI string to set for the HTTP request.</param>
        /// <return>The updated instance of HttpMessageBuilderSupport.</return>
        public HttpMessageBuilderSupport Uri(string uri)
        {
            httpMessage.Uri(uri);
            return this;
        }

        /// Adds a query parameter to the HTTP message being built.
        /// <param name="name">The name of the query parameter to add. Must not be null or empty.</param>
        /// <return>The updated instance of HttpMessageBuilderSupport.</return>
        public HttpMessageBuilderSupport QueryParam(string name)
        {
            httpMessage.QueryParam(name);
            return this;
        }

        /// Adds a query parameter to the HTTP request being built.
        /// <param name="name">The name of the query parameter. Must not be empty or null.</param>
        /// <param name="value">The value of the query parameter. Can be null if no value is provided.</param>
        /// <return>The updated instance of HttpMessageBuilderSupport.</return>
        public HttpMessageBuilderSupport QueryParam(string name, string value)
        {
            httpMessage.QueryParam(name, value);
            return this;
        }

        /// Sets the HTTP version for the message being built.
        /// <param name="version">The HTTP version to specify for the message.</param>
        /// <return>The updated instance of HttpMessageBuilderSupport.</return>
        public HttpMessageBuilderSupport Version(string version)
        {
            httpMessage.Version(version);
            return this;
        }

        /// Sets the HTTP message content type header in the builder.
        /// <param name="contentType">The content type header value to assign to the HTTP message.</param>
        /// <return>The updated instance of HttpMessageBuilderSupport.</return>
        public HttpMessageBuilderSupport ContentType(string contentType)
        {
            httpMessage.ContentType(contentType);
            return this;
        }

        /// Sets the HTTP "Accept" header for the message being built.
        /// <param name="accept">The value to assign to the "Accept" header.</param>
        /// <return>The updated instance of HttpMessageBuilderSupport.</return>
        public HttpMessageBuilderSupport Accept(string accept)
        {
            httpMessage.Accept(accept);
            return this;
        }

        /// Adds a cookie to the HTTP message being built.
        /// <param name="cookie">The Cookie instance to be added or updated in the HTTP message.</param>
        /// <return>The updated instance of HttpMessageBuilderSupport.</return>
        public HttpMessageBuilderSupport Cookie(Cookie cookie)
        {
            httpMessage.Cookie(cookie);
            return this;
        }
    }
}