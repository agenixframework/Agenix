#nullable enable
using System;
using Agenix.Core.Exceptions;
using Agenix.Core.Validation;
using log4net;

namespace Agenix.Core.Message;

public delegate void MessageProcessor(IMessage message, TestContext context);

/// <summary>
///     Defines the contract for processing messages within the given context.
/// </summary>
public interface IMessageProcessor : IMessageTransformer
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(IMessageProcessor));

    /// <summary>
    ///     Processes the given message payload within the specified test context.
    /// </summary>
    /// <param name="message">The message to process.</param>
    /// <param name="context">The context in which the message is being processed.</param>
    void Process(IMessage message, TestContext context);

    /// <summary>
    ///     Transforms the given message within the specified test context.
    /// </summary>
    /// <param name="message">The message to transform.</param>
    /// <param name="context">The context in which the message is being transformed.</param>
    /// <returns>The transformed message.</returns>
    new IMessage Transform(IMessage message, TestContext context)
    {
        Process(message, context);
        return message;
    }

    /// <summary>
    ///     Retrieves an instance of the specified message processor builder based on the provided validator.
    /// </summary>
    /// <param name="validator">The identifier of the message processor builder to retrieve.</param>
    /// <typeparam name="T">The type of IMessageProcessor.</typeparam>
    /// <typeparam name="TB">The type of the builder for the IMessageProcessor.</typeparam>
    /// <returns>An instance of the message processor builder, or null if it fails to resolve.</returns>
    public static IBuilder<T, TB>? Lookup<T, TB>(string validator)
        where T : IMessageProcessor where TB : IBuilder<T, TB>
    {
        try
        {
            switch (validator)
            {
                case "json-path-message-processor-builder":
                {
                    var instance = (TB)Activator.CreateInstance(typeof(JsonPathMessageProcessor.Builder))!;
                    return instance;
                }
            }
        }
        catch (CoreSystemException)
        {
            Log.Warn($"Failed to resolve message processor from resource '{validator}'");
        }

        return null;
    }

    /// <summary>
    ///     Interface IBuilder defines the contract for building instances of
    ///     IMessageProcessor implementations.
    /// </summary>
    new interface IBuilder<out T, TB> where T : IMessageProcessor where TB : IBuilder<T, TB>
    {
        /// <summary>
        ///     Builds new message processor instance.
        /// </summary>
        /// <returns></returns>
        T Build();
    }
}