using Agenix.Core;
using Agenix.Core.Endpoint;
using Agenix.Core.Exceptions;
using Agenix.Core.Message;
using Agenix.Core.Message.Correlation;
using Agenix.Core.Messaging;
using Agenix.Http.Interceptor;
using Agenix.Http.Message;
using log4net;

namespace Agenix.Http.Client;

/// <summary>
///     Http client sends messages via Http protocol to some Http server instance, defined by a request endpoint url.
///     Synchronous response messages are cached in local memory and receive operations are able to fetch responses from
///     this cache later on.
/// </summary>
public class HttpClient : AbstractEndpoint, IProducer, IReplyConsumer
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(HttpClient));

    /// <summary>
    ///     Store of reply messages
    /// </summary>
    private ICorrelationManager<IMessage> _correlationManager;
       

    public HttpClient(HttpEndpointConfiguration endpointConfiguration)
        : base(endpointConfiguration)
    {
        Name = nameof(HttpClient);
        _correlationManager =new PollingCorrelationManager<IMessage>(endpointConfiguration, "Reply message did not arrive yet");
    }
    
    /// <summary>
    ///     Default constructor initializing endpoint configuration.
    /// </summary>
    public HttpClient() : this(new HttpEndpointConfiguration())
    {
    }

    /// <summary>
    ///     Represents the configuration specific to an endpoint within the HttpClient.
    /// </summary>
    public override HttpEndpointConfiguration EndpointConfiguration => (HttpEndpointConfiguration) base.EndpointConfiguration;

    public void Send(IMessage message, TestContext context)
    {
        foreach (var interceptor in EndpointConfiguration.ClientHandlers
                     .OfType<LoggingClientHandler>()
                     .Where(interceptor => !interceptor.HasMessageListeners()))
            interceptor.SetMessageListener(context.MessageListeners);

        HttpMessage httpMessage;
        if (message is HttpMessage message1)
            httpMessage = message1;
        else
            httpMessage = new HttpMessage(message);

        var correlationKeyName = GetCorrelationKeyName();
        var correlationKey = EndpointConfiguration.Correlator.GetCorrelationKey(httpMessage);
        _correlationManager.SaveCorrelationKey(correlationKeyName, correlationKey, context);

        var endpointUri = GetEndpointUri(httpMessage);
        context.SetVariable(MessageHeaders.MessageReplyTo + "_" + correlationKeyName, endpointUri);

        Log.Info($"Sending HTTP message to: '{endpointUri}'");
        Log.Debug($"Message to send:\n{httpMessage.GetPayload<string>()}");

        var method = EndpointConfiguration.RequestMethod;
        if (httpMessage.GetRequestMethod() != null) method = httpMessage.GetRequestMethod();

        var httpRequestMessage =
            EndpointConfiguration.MessageConverter.ConvertOutbound(httpMessage, EndpointConfiguration, context);

        try
        {
            httpRequestMessage.RequestUri = new Uri(endpointUri!);
            httpRequestMessage.Method = method!;
            var httpResponseMessage = EndpointConfiguration.HttpClient.Send(httpRequestMessage);

            Log.Debug($"HTTP message was sent to endpoint: '{endpointUri}'");
            _correlationManager.Store(correlationKey,
                EndpointConfiguration.MessageConverter.ConvertInbound(httpResponseMessage, EndpointConfiguration,
                    context));
        }
        catch (Exception e)
        {
            Log.Warn("Caught HTTP Client exception!", e);
        }
    }

    /// <summary>
    ///     Receives a message using a correlation key derived from the context.
    /// </summary>
    /// <param name="context">The test context providing additional information required for receiving the message.</param>
    /// <returns>Returns an <see cref="IMessage" /> instance that matches the correlation key.</returns>
    public IMessage Receive(TestContext context)
    {
        return Receive(_correlationManager.GetCorrelationKey(GetCorrelationKeyName(), context), context);
    }

    /// <summary>
    ///     Receives a message from the endpoint using a specified correlation key and context, with a timeout.
    /// </summary>
    /// <param name="context">The context in which the message is being received.</param>
    /// <param name="timeout">The time in milliseconds to wait for the message before timing out.</param>
    /// <returns>The received message, or null if the timeout expires before a message is received.</returns>
    public virtual IMessage Receive(TestContext context, long timeout)
    {
        return Receive(_correlationManager.GetCorrelationKey(GetCorrelationKeyName(), context), context, timeout);
    }

    /// <summary>
    ///     Receives a message from the endpoint using the provided selector and test context.
    /// </summary>
    /// <param name="selector">The correlation key used to identify the message.</param>
    /// <param name="context">The test context which may contain details for message correlation.</param>
    /// <returns>The received message as an <see cref="IMessage" />.</returns>
    public IMessage Receive(string selector, TestContext context)
    {
        return Receive(selector, context, EndpointConfiguration.Timeout);
    }

    /// <summary>
    ///     Receives a message based on the specified selector within a given timeout period.
    /// </summary>
    /// <param name="selector">The selector criteria to filter the messages.</param>
    /// <param name="context">The test context that provides necessary execution environment information.</param>
    /// <param name="timeout">The maximum amount of time to wait for a message before timing out.</param>
    /// <returns>The message that matches the selector criteria.</returns>
    /// <exception cref="MessageTimeoutException">Thrown when a message cannot be received within the specified timeout.</exception>
    public IMessage Receive(string selector, TestContext context, long timeout)
    {
        var message = _correlationManager.Find(selector, timeout);

        var endpointUri = context.GetVariables().ContainsKey(MessageHeaders.MessageReplyTo + "_" + selector)
            ? context.GetVariable(MessageHeaders.MessageReplyTo + "_" + selector)
            : Name;

        if (message == null) throw new MessageTimeoutException(timeout, endpointUri);

        return message;
    }


    /// <summary>
    ///     Creates and returns an instance of a producer.
    /// </summary>
    /// <returns>
    ///     The current instance as an implementation of the IProducer interface.
    /// </returns>
    public override IProducer CreateProducer()
    {
        return this;
    }

    /// <summary>
    ///     Creates and returns an instance of an ISelectiveConsumer.
    /// </summary>
    /// <returns>An instance of ISelectiveConsumer, specifically the current instance of HttpClient.</returns>
    public override ISelectiveConsumer CreateConsumer()
    {
        return this;
    }

    /// <summary>
    ///     Sets the correlation manager used for managing message correlation.
    /// </summary>
    /// <param name="correlationManager">The correlation manager to be set for message correlation operations.</param>
    public void SetCorrelationManager(ICorrelationManager<IMessage> correlationManager)
    {
        _correlationManager = correlationManager;
    }

    /// <summary>
    ///     Retrieves the endpoint URI for the given HTTP message, using the configured endpoint URI resolver if available.
    /// </summary>
    /// <param name="httpMessage">The HTTP message for which the endpoint URI needs to be determined.</param>
    /// <returns>The resolved endpoint URI if the resolver is available, otherwise the default request URL.</returns>
    private string? GetEndpointUri(HttpMessage httpMessage)
    {
        return EndpointConfiguration.EndpointUriResolver != null
            ? EndpointConfiguration.EndpointUriResolver.ResolveEndpointUri(httpMessage,
                EndpointConfiguration.RequestUrl)
            : EndpointConfiguration.RequestUrl;
    }

    /// <summary>
    ///     Retrieves the correlation key name using the configured correlator and the current endpoint name.
    /// </summary>
    /// <returns>The correlation key name derived from the consumer name associated with the current endpoint.</returns>
    private string GetCorrelationKeyName()
    {
        return EndpointConfiguration.Correlator.GetCorrelationKeyName(this.Name);
    }
}