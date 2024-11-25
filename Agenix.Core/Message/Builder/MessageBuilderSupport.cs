using System.Collections.Generic;
using System.IO;
using System.Text;
using Agenix.Core.Common;
using Agenix.Core.Exceptions;
using Agenix.Core.Spi;
using Agenix.Core.Util;
using Agenix.Core.Validation.builder;
using Agenix.Core.Variable;

namespace Agenix.Core.Message.Builder;

public abstract class MessageBuilderSupport<T, TB, TS> : ITestActionBuilder<T>, IReferenceResolverAware
    where T : ITestAction
    where TB : MessageActionBuilder<T, TS, TB>
    where TS : MessageBuilderSupport<T, TB, TS>
{
    protected readonly TB _delegate;

    protected readonly TS _self;

    protected IMessageBuilder _messageBuilder = new DefaultMessageBuilder();

    protected string _messageType = CoreSettings.DefaultMessageType;

    /// <summary>
    ///     Provides base support for building messages, tying together
    ///     various components of the message construction process.
    /// </summary>
    /// <typeparam name="T">The type of the test action.</typeparam>
    /// <typeparam name="TB">The type of the message action builder.</typeparam>
    /// <typeparam name="TS">The type of the message builder support.</typeparam>
    protected MessageBuilderSupport(TB dlg)
    {
        _self = (TS)this;
        _delegate = dlg;
    }

    /// <summary>
    ///     Sets the reference resolver for the message builder support, ensuring that any needed references can be resolved at
    ///     runtime.
    /// </summary>
    /// <param name="referenceResolver">The reference resolver to be set.</param>
    public void SetReferenceResolver(IReferenceResolver referenceResolver)
    {
        _delegate.SetReferenceResolver(referenceResolver);
    }


    /// <summary>
    ///     Completes the building process and constructs the final test action.
    /// </summary>
    /// <returns>The constructed test action of type T.</returns>
    public T Build()
    {
        return _delegate.Build();
    }

    /// <summary>
    ///     Sets a custom message builder for the message construction process.
    /// </summary>
    /// <param name="messageBuilder">The custom <see cref="IMessageBuilder" /> to use for building messages.</param>
    /// <returns>The current instance of the message builder support.</returns>
    public TS From(IMessageBuilder messageBuilder)
    {
        _messageBuilder = messageBuilder;
        return _self;
    }

    /// <summary>
    ///     Assigns the provided control message to the internal message builder and sets the message type.
    /// </summary>
    /// <param name="controlMessage">The control message to assign.</param>
    /// <returns>Returns an instance of the current builder support class.</returns>
    public TS From(IMessage controlMessage)
    {
        _messageBuilder = StaticMessageBuilder.WithMessage(controlMessage);
        Type(controlMessage.GetType());
        return _self;
    }


    /// <summary>
    ///     Sets the type of the message to be built.
    /// </summary>
    /// <param name="messageType">The type of the message.</param>
    /// <return>Returns an instance of the message builder support class.</return>
    public TS Type(string messageType)
    {
        _messageType = messageType;
        return _self;
    }

    /// <summary>
    ///     Sets the type of the message to be built.
    /// </summary>
    /// <param name="messageType">The type of the message.</param>
    /// <returns>Returns an instance of the message builder support class.</returns>
    public TS Type(MessageType messageType)
    {
        Type(messageType.ToString());
        return _self;
    }

    /// <summary>
    ///     Sets the body of the message using the provided message payload builder.
    /// </summary>
    /// <param name="messagePayloadBuilder">The payload builder to use for constructing the message body.</param>
    /// <returns>The current instance of the message builder support class with the body set.</returns>
    public TS Body(MessagePayloadBuilder messagePayloadBuilder)
    {
        return Body(new DelegatingMessagePayloadBuilder(messagePayloadBuilder));
    }


    /// <summary>
    ///     Associates a payload builder with the current message builder, enabling custom payload construction.
    /// </summary>
    /// <param name="payloadBuilder">An instance of <see cref="IMessagePayloadBuilder" /> used to build the message payload.</param>
    /// <returns>The updated instance of <typeparamref name="TS" /> to allow method chaining.</returns>
    /// <exception cref="CoreSystemException">Thrown if the message builder does not support setting a payload builder.</exception>
    public TS Body(IMessagePayloadBuilder payloadBuilder)
    {
        if (_messageBuilder is IWithPayloadBuilder withPayloadBuilder)
            withPayloadBuilder.SetPayloadBuilder(payloadBuilder);
        else
            throw new CoreSystemException("Unable to set payload builder on message builder type: " +
                                          _messageBuilder.GetType());
        return _self;
    }

    /// <summary>
    ///     Sets the payload of the message to the specified string value.
    /// </summary>
    /// <param name="payload">The string payload to be set for the message.</param>
    /// <returns>The updated instance of <typeparamref name="TS" /> to allow method chaining.</returns>
    /// <exception cref="CoreSystemException">Thrown if the message builder does not support setting a payload.</exception>
    public TS Body(string payload)
    {
        Body(new DefaultPayloadBuilder(payload));
        return _self;
    }

    /// <summary>
    ///     Sets the body of the message using the content from the specified resource.
    /// </summary>
    /// <param name="payloadResource">The resource containing the payload.</param>
    /// <returns>The instance of the message builder support with the updated body.</returns>
    public TS Body(IResource payloadResource)
    {
        return Body(payloadResource, FileUtils.GetDefaultCharset());
    }

    /// <summary>
    ///     Sets the body of the message using the provided payload resource and character encoding.
    /// </summary>
    /// <param name="payloadResource">The resource containing the payload data.</param>
    /// <param name="charset">The character encoding used to read the payload.</param>
    /// <returns>The builder instance with the updated payload.</returns>
    /// <exception cref="CoreSystemException">Thrown if there is an error reading the payload resource.</exception>
    public TS Body(IResource payloadResource, Encoding charset)
    {
        try
        {
            Body(FileUtils.ReadToString(payloadResource, charset));
        }
        catch (IOException e)
        {
            throw new CoreSystemException("Failed to read payload resource", e);
        }

        return _self;
    }

    /// <summary>
    ///     Adds a header to the message being built.
    /// </summary>
    /// <param name="name">The name of the header to be added.</param>
    /// <param name="value">The value of the header to be added.</param>
    /// <returns>An instance of the current message builder type.</returns>
    /// <exception cref="CoreSystemException">Thrown when the message builder does not support adding headers.</exception>
    public TS Header(string name, object value)
    {
        if (_messageBuilder is IWithHeaderBuilder withHeaderBuilder)
            withHeaderBuilder.AddHeaderBuilder(new DefaultHeaderBuilder(new Dictionary<string, object>
                { { name, value } }));
        else
            throw new CoreSystemException($"Unable to set message header on builder type: {_messageBuilder.GetType()}");
        return _self;
    }

    /// <summary>
    ///     Sets multiple headers for the message being built by the builder.
    /// </summary>
    /// <param name="headers">A dictionary containing the header names and their corresponding values.</param>
    /// <returns>The current instance of the message builder support.</returns>
    /// <exception cref="CoreSystemException">Thrown when the message builder type does not support adding headers.</exception>
    public TS Headers(Dictionary<string, object> headers)
    {
        if (_messageBuilder is IWithHeaderBuilder withHeaderBuilder)
            withHeaderBuilder.AddHeaderBuilder(new DefaultHeaderBuilder(headers));
        else
            throw new CoreSystemException($"Unable to set message header on builder type: {_messageBuilder.GetType()}");
        return _self;
    }

    /// <summary>
    ///     Adds headers to the message from the provided resource.
    /// </summary>
    /// <param name="resource">The resource containing the headers to be added.</param>
    /// <returns>An instance of the message builder support class.</returns>
    public TS Header(IResource resource)
    {
        return Header(resource, FileUtils.GetDefaultCharset());
    }

    /// <summary>
    ///     Sets the header for the message using the data read from the specified resource and charset.
    /// </summary>
    /// <param name="resource">The resource from which the header data is read.</param>
    /// <param name="charset">The character encoding used to read the resource.</param>
    /// <returns>The current instance of the message builder support for method chaining.</returns>
    /// <exception cref="CoreSystemException">
    ///     Thrown if the message builder does not support adding header builders or if reading the resource fails.
    /// </exception>
    public TS Header(IResource resource, Encoding charset)
    {
        try
        {
            if (_messageBuilder is IWithHeaderBuilder withHeaderBuilder)
                withHeaderBuilder.AddHeaderBuilder(
                    new DefaultHeaderDataBuilder(FileUtils.ReadToString(resource, charset)));
            else
                throw new CoreSystemException("Unable to set message header data on builder type: " +
                                              _messageBuilder.GetType());
        }
        catch (IOException e)
        {
            throw new CoreSystemException("Failed to read header resource", e);
        }

        return _self;
    }

    /// <summary>
    ///     Sets the header data for the message using the specified header data builder.
    /// </summary>
    /// <param name="headerDataBuilder">The builder to construct the header data.</param>
    /// <returns>The current instance of the message builder support.</returns>
    public TS Header(IMessageHeaderDataBuilder headerDataBuilder)
    {
        if (_messageBuilder is IWithHeaderBuilder withHeaderBuilder)
            withHeaderBuilder.AddHeaderBuilder(headerDataBuilder);
        else
            throw new CoreSystemException(
                $"Unable to set message header data on builder type: {_messageBuilder.GetType()}");
        return _self;
    }

    /// <summary>
    ///     Provides base support for building messages, tying together
    ///     various components of the message construction process.
    /// </summary>
    /// <typeparam name="T">The type of the test action</typeparam>
    /// <typeparam name="TB">The type of the message action builder</typeparam>
    /// <typeparam name="TS">The type of the message builder support</typeparam>
    /// <param name="headerDataBuilder">The message header data builder</param>
    /// <returns>Returns an instance of the derived message builder support</returns>
    public TS Header(MessageHeaderDataBuilder headerDataBuilder)
    {
        return Header(new DelegatingMessageHeaderDataBuilder(headerDataBuilder));
    }

    /// <summary>
    ///     Sets a header for the message using the provided string data.
    /// </summary>
    /// <param name="data">The header data to set.</param>
    /// <returns>The current instance of the message builder support.</returns>
    public TS Header(string data)
    {
        Header(new DefaultHeaderDataBuilder(data));
        return _self;
    }

    /// <summary>
    ///     Sets the name of the message using the provided name string.
    /// </summary>
    /// <param name="name">The name to be set on the message.</param>
    /// <returns>The current instance of the message builder support.</returns>
    /// <exception cref="CoreSystemException">Thrown when the message builder does not support setting a name.</exception>
    public TS Name(string name)
    {
        if (_messageBuilder is INamed named)
            named.SetName(name);
        else
            throw new CoreSystemException("Unable to set message name on builder type: " + _messageBuilder.GetType());
        return _self;
    }

    /// <summary>
    ///     Adds a processor to handle the message processing logic.
    /// </summary>
    /// <param name="processor">The processor responsible for handling the message processing.</param>
    /// <returns>The updated instance of the message builder support.</returns>
    public virtual TS Process(IMessageProcessor processor)
    {
        _delegate.Process(processor);
        return _self;
    }

    /// <summary>
    ///     Processes a message using the specified message processor.
    /// </summary>
    /// <param name="builder">The message processor to use for processing the message.</param>
    /// <returns>A reference to this instance with the processing applied.</returns>
    public TS Process<B>(IMessageProcessor.IBuilder<IMessageProcessor, B> builder)
        where B : IMessageProcessor.IBuilder<IMessageProcessor, B>
    {
        return Process(builder.Build());
    }

    /// <summary>
    ///     Processes the specified message processor.
    /// </summary>
    /// <param name="adapter">The message processor to process.</param>
    /// <returns>The current instance of <see cref="TS" /> to allow for method chaining.</returns>
    public TS Process(IMessageProcessorAdapter adapter)
    {
        return Process(adapter.AsProcessor());
    }

    /// <summary>
    ///     Allows for embedding a variable extractor into the message processing pipeline.
    /// </summary>
    /// <param name="extractor">The variable extractor to be used.</param>
    /// <returns>The current instance of the message builder support for chaining purposes.</returns>
    public TS Extract(IVariableExtractor extractor)
    {
        return Process(extractor);
    }

    /// <summary>
    ///     Configures the current message builder to extract variables
    ///     using the specified variable extractor adapter.
    /// </summary>
    /// <param name="adapter">The adapter used to convert to a variable extractor.</param>
    /// <returns>The current message builder with the applied extraction settings.</returns>
    public TS Extract(IVariableExtractorAdapter adapter)
    {
        return Extract(adapter.AsExtractor());
    }

    /// <summary>
    ///     Extracts variables using the provided variable extractor builder.
    /// </summary>
    /// <param name="builder">The variable extractor builder used to build the variable extractor.</param>
    /// <returns>The current instance of the message builder support.</returns>
    public TS Extract<B>(IVariableExtractor.IBuilder<IVariableExtractor, B> builder)
        where B : IVariableExtractor.IBuilder<IVariableExtractor, B>
    {
        return Extract(builder.Build());
    }

    /// <summary>
    ///     Associates the given reference resolver with the message builder.
    /// </summary>
    /// <param name="referenceResolver">The reference resolver to be associated.</param>
    /// <returns>The current instance of the message builder support for method chaining.</returns>
    public TS WithReferenceResolver(IReferenceResolver referenceResolver)
    {
        _delegate.WithReferenceResolver(referenceResolver);
        return _self;
    }

    /// <summary>
    ///     Retrieves the current message builder used for constructing messages.
    /// </summary>
    /// <returns>The <see cref="IMessageBuilder" /> instance that is used for message building.</returns>
    public IMessageBuilder GetMessageBuilder()
    {
        return _messageBuilder;
    }

    /// <summary>
    ///     Retrieves the message type that is currently set in the message builder.
    /// </summary>
    /// <returns>The type of the message as a string.</returns>
    public string GetMessageType()
    {
        return _messageType;
    }
}