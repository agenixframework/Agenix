#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Agenix.Api.Common;
using Agenix.Api.Context;
using Agenix.Api.Endpoint;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Variable;
using Agenix.Api.Variable.Dictionary;
using Agenix.Core.Message.Builder;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Actions;

/// <summary>
///     This action sends a message to a specified message endpoint. The action holds a reference to an Endpoint, which is
///     capable of the message transport implementation. So the action is independent of the message transport
///     configuration.
/// </summary>
public class SendMessageAction : AbstractTestAction, ICompletable
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(SendMessageAction));

    /// <summary>
    ///     Represents the completion status of the action represented by a TaskCompletionSource.
    /// </summary>
    private TaskCompletionSource<TestContext> _finished;

    public SendMessageAction(Builder builder) : base(builder.GetName() ?? "send", builder.GetDescription() ?? "")
    {
        ForkMode = builder.ForkMode;
        Endpoint = builder.GetEndpoint();
        EndpointUri = builder.GetEndpointUri();
        IsSchemaValidation = builder.GetMessageBuilderSupport().IsSchemaValidation();
        Schema = builder.GetMessageBuilderSupport().GetSchema;
        SchemaRepository = builder.GetMessageBuilderSupport().GetSchemaRepository;
        VariableExtractors = builder.GetVariableExtractors();
        Processors = builder.GetMessageProcessors();
        MessageBuilder = builder.GetMessageBuilderSupport().GetMessageBuilder();
        MessageType = builder.GetMessageBuilderSupport().GetMessageType();
        DataDictionary = builder.GetMessageBuilderSupport().DataDictionary;
    }

    public IDataDictionary DataDictionary { get; set; }

    /// <summary>
    ///     Message endpoint instance
    /// </summary>
    public IEndpoint Endpoint { get; }

    /// <summary>
    ///     Indicates whether the message-sending action should be forked, allowing other actions to proceed while waiting for
    ///     a synchronous response.
    /// </summary>
    public bool ForkMode { get; }

    /// <summary>
    ///     The URI of the endpoint where the message will be sent.
    /// </summary>
    public string EndpointUri { get; }

    /// <summary>
    ///     Indicates whether schema validation is enabled for the message. When set to true, the message schema
    ///     validation will be performed during the execution of the action.
    /// </summary>
    public bool IsSchemaValidation { get; }

    /// <summary>
    ///     Represents the schema associated with the message for validation purposes.
    ///     This defines the structure and rules that the message should conform to
    ///     when being used in the action.
    /// </summary>
    public string Schema { get; }

    /// <summary>
    ///     Represents the repository location or reference for schema validation.
    /// </summary>
    public string SchemaRepository { get; }

    /// <summary>
    ///     List of variable extractors responsible for creating variables from received message content
    /// </summary>
    public List<IVariableExtractor> VariableExtractors { get; }

    /// <summary>
    ///     List of message processors responsible for manipulating messages to be sent
    /// </summary>
    public List<IMessageProcessor> Processors { get; }

    /// <summary>
    ///     Builder for constructing control messages.
    /// </summary>
    public IMessageBuilder MessageBuilder { get; }

    /// <summary>
    ///     The message type to send in this action - this information is needed to find proper
    ///     message construction processors for this message.
    /// </summary>
    public string MessageType { get; set; }

    /// <summary>
    ///     Determines whether the sending message action is completed.
    /// </summary>
    /// <param name="context">The test context containing relevant information and processors.</param>
    /// <returns>True if the sending message action is completed or disabled, otherwise false.</returns>
    public bool IsDone(TestContext context)
    {
        return (_finished?.Task.IsCompleted ?? IsDisabled(context)) || IsDisabled(context);
    }

    /// <summary>
    ///     Creates a message using the provided context and message type.
    /// </summary>
    /// <param name="context">The test context containing relevant information and processors.</param>
    /// <param name="messageType">The type of message to create.</param>
    /// <returns>The created message after being processed by available message processors.</returns>
    protected IMessage CreateMessage(TestContext context, string messageType)
    {
        var message = MessageBuilder.Build(context, messageType);

        if (message.Payload == null)
        {
            return message;
        }

        foreach (var processor in context.GetMessageProcessors(MessageDirection.OUTBOUND))
        {
            processor.Process(message, context);
        }

        DataDictionary?.Process(message, context);

        foreach (var processor in Processors)
        {
            processor.Process(message, context);
        }

        return message;
    }

    /// <summary>
    ///     Creates or gets the message endpoint instance.
    /// </summary>
    /// <param name="context">the test context</param>
    /// <returns>the message endpoint</returns>
    /// <exception cref="AgenixSystemException"></exception>
    public IEndpoint GetOrCreateEndpoint(TestContext context)
    {
        if (Endpoint != null)
        {
            return Endpoint;
        }

        if (!string.IsNullOrWhiteSpace(EndpointUri))
        {
            return context.EndpointFactory.Create(EndpointUri, context);
        }

        throw new AgenixSystemException("Neither endpoint nor endpoint uri is set properly!");
    }

    /// <summary>
    ///     Determines whether the action is disabled based on the given test context.
    /// </summary>
    /// <param name="context">The test context containing relevant information and processors.</param>
    /// <returns>True if the action is disabled, otherwise false.</returns>
    public override bool IsDisabled(TestContext context)
    {
        var messageEndpoint = GetOrCreateEndpoint(context);

        return base.IsDisabled(context);
    }

    /// <summary>
    ///     Executes the message sending action with the provided test context.
    /// </summary>
    /// <param name="context">The test context containing relevant information and processors.</param>
    public override void DoExecute(TestContext context)
    {
        var message = CreateMessage(context, MessageType);
        _finished = new TaskCompletionSource<TestContext>();

        _finished.Task.ContinueWith(task =>
        {
            if (task is { IsFaulted: true, Exception: not null })
            {
                Log.LogWarning("Failure in forked send action: {}", task.Exception.Message);
            }
            else
            {
                foreach (var ctxEx in context.GetExceptions())
                {
                    Log.LogWarning(ctxEx, ctxEx.Message);
                }
            }
        });

        // Extract variables from before sending message so we can save dynamic message ids
        foreach (var variableExtractor in VariableExtractors)
        {
            variableExtractor.ExtractVariables(message, context);
        }

        var messageEndpoint = GetOrCreateEndpoint(context);

        context.MessageStore.StoreMessage(
            !string.IsNullOrWhiteSpace(message.Name)
                ? message.Name
                : context.MessageStore.ConstructMessageName(this, messageEndpoint), message);

        if (ForkMode)
        {
            Log.LogDebug("Forking message sending action ...");

            Task.Run(() =>
            {
                try
                {
                    ValidateMessage(message, context);
                    messageEndpoint.CreateProducer().Send(message, context);
                }
                catch (Exception e)
                {
                    if (e is AgenixSystemException runtimeEx)
                    {
                        context.AddException(runtimeEx);
                    }
                    else
                    {
                        context.AddException(new AgenixSystemException(e.Message));
                    }
                }
                finally
                {
                    _finished.SetResult(context);
                }
            });
        }
        else
        {
            try
            {
                ValidateMessage(message, context);
                messageEndpoint.CreateProducer().Send(message, context);
            }
            finally
            {
                _finished.SetResult(context);
            }
        }
    }

    /// <summary>
    ///     Validate the message against registered schema validators.
    /// </summary>
    private void ValidateMessage(IMessage message, TestContext context)
    {
        foreach (var validator in context.MessageValidatorRegistry.SchemaValidators.Values
                     .Where(validator => validator.CanValidate(message, IsSchemaValidation)))
        {
            validator.Validate(message, context, SchemaRepository, Schema);
        }
    }

    /// <summary>
    ///     Builder for constructing and sending message actions.
    /// </summary>
    /// <typeparam name="T">Type of the action being built.</typeparam>
    /// <typeparam name="TM">Builder support type for message construction.</typeparam>
    /// <typeparam name="TB">Builder type for the specific send message action.</typeparam>
    public class Builder : SendMessageActionBuilder<SendMessageAction, SendMessageActionBuilderSupport, Builder>
    {
        /// <summary>
        ///     Sends a message action to the specified endpoint.
        /// </summary>
        /// <param name="context">The test context containing relevant information for the action.</param>
        /// <param name="messageType">The type of the message to be sent.</param>
        /// <returns>A builder for constructing the message and sending it.</returns>
        public static Builder Send()
        {
            return new Builder();
        }

        /// <summary>
        ///     Sends a message action to the specified endpoint.
        /// </summary>
        /// <param name="context">The test context containing relevant information for the action.</param>
        /// <param name="messageType">The type of the message to be sent.</param>
        /// <returns>A builder for constructing the message and sending it.</returns>
        public static Builder Send(IEndpoint messageEndpoint)
        {
            var builder = new Builder();
            builder.Endpoint(messageEndpoint);
            return builder;
        }

        /// <summary>
        ///     Sends a message action to the specified endpoint.
        /// </summary>
        /// <param name="context">The test context containing relevant information for the action.</param>
        /// <param name="messageType">The type of the message to be sent.</param>
        /// <returns>A builder for constructing the message and sending it.</returns>
        public static Builder Send(string messageEndpointUri)
        {
            var builder = new Builder();
            builder.Endpoint(messageEndpointUri);
            return builder;
        }

        /// <summary>
        ///     Retrieves the message builder support instance, initializing it if necessary.
        /// </summary>
        /// <returns>A SendMessageActionBuilderSupport instance associated with the current builder.</returns>
        public override SendMessageActionBuilderSupport GetMessageBuilderSupport()
        {
            if (messageBuilderSupport == null)
            {
                messageBuilderSupport = new SendMessageActionBuilderSupport(self);
            }

            return base.GetMessageBuilderSupport();
        }

        /// <summary>
        ///     Constructs and initializes a new instance of the SendMessageAction class using the builder's configuration.
        /// </summary>
        /// <returns>A newly created SendMessageAction configured with the builder's settings.</returns>
        protected override SendMessageAction DoBuild()
        {
            return new SendMessageAction(this);
        }
    }

    /// <summary>
    ///     Builder support specifically designed for constructing and sending message actions
    ///     within the context of a SendMessageAction.
    /// </summary>
    public class SendMessageActionBuilderSupport(Builder dlg)
        : SendMessageBuilderSupport<SendMessageAction, Builder, SendMessageActionBuilderSupport>(dlg);

    /// <summary>
    ///     Base send message action builder also used by subclasses of base send message action.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TM"></typeparam>
    /// <typeparam name="TB"></typeparam>
    public abstract class SendMessageActionBuilder<T, TM, TB> : MessageActionBuilder<T, TM, TB>
        where T : SendMessageAction
        where TM : SendMessageBuilderSupport<T, TB, TM>
        where TB : SendMessageActionBuilder<T, TM, TB>
    {
        protected internal bool ForkMode;

        /// <summary>
        ///     Sets the fork mode for this send action builder.
        /// </summary>
        /// <param name="forkMode"></param>
        /// <returns></returns>
        public TB Fork(bool forkMode)
        {
            ForkMode = forkMode;
            return self;
        }


        /// <summary>
        ///     Builds the SendMessageAction by initializing the messageBuilderSupport if necessary.
        /// </summary>
        /// <returns>The constructed SendMessageAction instance.</returns>
        public override T Build()
        {
            if (messageBuilderSupport == null)
            {
                messageBuilderSupport = GetMessageBuilderSupport();
            }

            if (referenceResolver != null)
            {
                if (messageBuilderSupport.DataDictionaryName != null)
                {
                    messageBuilderSupport.Dictionary(
                        referenceResolver.Resolve<IDataDictionary>(messageBuilderSupport.DataDictionaryName));
                }
            }

            return DoBuild();
        }
    }
}
