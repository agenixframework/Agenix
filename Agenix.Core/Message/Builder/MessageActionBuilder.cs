using System.Collections.Generic;
using Agenix.Api;
using Agenix.Api.Endpoint;
using Agenix.Api.Message;
using Agenix.Api.Variable;
using Agenix.Core.Spi;

namespace Agenix.Core.Message.Builder;

/// Abstract class that provides a fluent API for constructing message-based actions.
/// Facilitates advanced configurations such as message processing, transformation,
/// endpoint assignment, and integration with variable extraction mechanisms.
/// @param
/// <T>
///     the type of the test action.
///     @param
///     <TM>
///         the type of the message builder support.
///         @param
///         <TB>
///             the type of the message action builder.
public abstract class MessageActionBuilder<T, TM, TB> : AbstractTestActionBuilder<T, TB>, IReferenceResolverAware
    where T : ITestAction
    where TM : MessageBuilderSupport<T, TB, TM>
    where TB : MessageActionBuilder<T, TM, TB>
{
    protected readonly List<IMessageProcessor> messageProcessors = [];

    protected readonly List<IVariableExtractor> variableExtractors = [];

    protected IEndpoint _endpoint;
    protected string _endpointUri;

    protected TM messageBuilderSupport;

    /**
     * Poco bean reference resolver
     */
    protected IReferenceResolver referenceResolver;

    /// <summary>
    ///     Sets the reference resolver for the action builder.
    /// </summary>
    /// <param name="referenceResolver">The reference resolver to be used by the builder.</param>
    public void SetReferenceResolver(IReferenceResolver referenceResolver)
    {
        this.referenceResolver = referenceResolver;
    }

    public TM Message(IMessageBuilder messageBuilder)
    {
        return GetMessageBuilderSupport().From(messageBuilder);
    }

    /// <summary>
    ///     Sets the message for the MessageActionBuilder using the given IMessage instance.
    /// </summary>
    /// <param name="message">The IMessage instance that contains the message to be used.</param>
    /// <returns>Returns the MessageBuilderSupport instance configured with the given message.</returns>
    public TM Message(IMessage message)
    {
        return GetMessageBuilderSupport().From(message);
    }

    /// <summary>
    ///     Transforms the message using the given IMessageProcessor.
    /// </summary>
    /// <param name="processor">The IMessageProcessor that will process the message.</param>
    /// <returns>Returns an instance of the current action builder.</returns>
    public TB Transform(IMessageProcessor processor)
    {
        return Process(processor);
    }

    /// <summary>
    ///     Transforms the message via the provided IMessageProcessorAdapter.
    /// </summary>
    /// <param name="adapter">The IMessageProcessorAdapter that will adapt and process the message.</param>
    /// <returns>Returns an instance of the current action builder.</returns>
    public TB Transform(IMessageProcessorAdapter adapter)
    {
        return Process(adapter.AsProcessor());
    }

    /// <summary>
    ///     Transforms the message using the provided MessageProcessorAdapter.
    /// </summary>
    /// <param name="adapter">The MessageProcessorAdapter that defines how to process the message.</param>
    /// <returns>Returns an instance of the current action builder.</returns>
    public TB Transform(MessageProcessorAdapter adapter)
    {
        return Process(adapter.Invoke());
    }

    /// <summary>
    ///     Transforms the message using the given IMessageProcessor.
    /// </summary>
    /// <param name="processor">The IMessageProcessor that will process and transform the message.</param>
    /// <returns>Returns an instance of the current action builder.</returns>
    /// <summary>
    ///     Transforms the message using the given IMessageProcessorAdapter.
    /// </summary>
    /// <param name="adapter">The IMessageProcessorAdapter that will process and transform the message.</param>
    /// <returns>Returns an instance of the current action builder.</returns>
    /// <summary>
    ///     Transforms the message using the given MessageProcessorAdapter.
    /// </summary>
    /// <param name="adapter">The MessageProcessorAdapter that will process and transform the message.</param>
    /// <returns>Returns an instance of the current action builder.</returns>
    /// <summary>
    ///     Transforms the message using the given builder for IMessageProcessor.
    /// </summary>
    /// <param name="builder">The builder that will create an instance of IMessageProcessor.</param>
    /// <returns>Returns an instance of the current action builder.</returns>
    public TB Transform<B>(IMessageProcessor.IBuilder<IMessageProcessor, B> builder)
        where B : IMessageProcessor.IBuilder<IMessageProcessor, B>
    {
        return Transform(builder.Build());
    }

    /// <summary>
    ///     Adds a message processor built from the given builder to the message action.
    /// </summary>
    /// <param name="builder">The builder that constructs an instance of IMessageProcessor.</param>
    /// <returns>Returns an instance of the current action builder.</returns>
    public TB Process<B>(IMessageProcessor.IBuilder<IMessageProcessor, B> builder)
        where B : IMessageProcessor.IBuilder<IMessageProcessor, B>
    {
        return Transform(builder.Build());
    }


    /// <summary>
    ///     Processes the message using the provided message processor builder.
    /// </summary>
    /// <param name="adapter">The builder that creates an instance of IMessageProcessor to process the message.</param>
    /// <returns>Returns an instance of the current action builder.</returns>
    public TB Process(IMessageProcessorAdapter adapter)
    {
        return Process(adapter.AsProcessor());
    }

    /// <summary>
    ///     Adds an IMessageProcessor to the list of processors or extractors depending on its type.
    /// </summary>
    /// <param name="processor">The IMessageProcessor instance to be added.</param>
    /// <returns>Returns an instance of the current action builder.</returns>
    public virtual TB Process(IMessageProcessor processor)
    {
        if (processor is IVariableExtractor variableExtractor)
            variableExtractors.Add(variableExtractor);
        else
            messageProcessors.Add(processor);
        return self;
    }

    /// <summary>
    ///     Configures the reference resolver for the message action builder.
    /// </summary>
    /// <param name="referenceResolver">The reference resolver to be used by the builder.</param>
    /// <returns>The current instance of the message action builder with the reference resolver set.</returns>
    public TB WithReferenceResolver(IReferenceResolver referenceResolver)
    {
        this.referenceResolver = referenceResolver;
        return self;
    }

    /// <summary>
    ///     Sets the message endpoint to send messages to.
    /// </summary>
    /// <param name="messageEndpoint"></param>
    /// <returns></returns>
    public TB Endpoint(IEndpoint messageEndpoint)
    {
        _endpoint = messageEndpoint;
        return self;
    }

    /// <summary>
    ///     Sets the message endpoint URI.
    /// </summary>
    /// <param name="messageEndpointUri">The URI of the message endpoint.</param>
    /// <returns>A reference to this instance.</returns>
    public TB Endpoint(string messageEndpointUri)
    {
        _endpointUri = messageEndpointUri;
        return self;
    }

    /// <summary>
    ///     Retrieves the message builder support instance for configuring and constructing message-related actions.
    /// </summary>
    /// <returns>The message builder support instance.</returns>
    public TM Message()
    {
        return GetMessageBuilderSupport();
    }

    /// <summary>
    ///     Retrieves the message builder support instance for building message actions.
    /// </summary>
    /// <returns>The message builder support instance.</returns>
    public virtual TM GetMessageBuilderSupport()
    {
        return messageBuilderSupport;
    }

    /// <summary>
    ///     Retrieves the current message endpoint.
    /// </summary>
    /// <returns>The current <see cref="IEndpoint" /> instance.</returns>
    public IEndpoint GetEndpoint()
    {
        return _endpoint;
    }

    /// <summary>
    ///     Retrieves the URI of the message endpoint.
    /// </summary>
    /// <returns>
    ///     The URI of the message endpoint as a string.
    /// </returns>
    public string GetEndpointUri()
    {
        return _endpointUri;
    }

    /// <summary>
    ///     Retrieves the list of variable extractors.
    /// </summary>
    /// <returns>List of variable extractors.</returns>
    public List<IVariableExtractor> GetVariableExtractors()
    {
        return variableExtractors;
    }

    /// <summary>
    ///     Retrieves the list of message processors configured for this action builder.
    /// </summary>
    /// <returns>
    ///     A list of IMessageProcessor instances.
    /// </returns>
    public List<IMessageProcessor> GetMessageProcessors()
    {
        return messageProcessors;
    }

    /// Build method implemented by subclasses.
    /// @return
    /// /
    protected abstract T DoBuild();
}