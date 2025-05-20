using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Util;
using Agenix.Core.Spi;
using log4net;

namespace Agenix.Api.Message;

public delegate void MessageProcessor(IMessage message, TestContext context);

/// <summary>
///     Defines the contract for processing messages within the given context.
/// </summary>
public interface IMessageProcessor : IMessageTransformer
{
    /// <summary>
    ///     Represents the path used to locate resources for the message processor.
    /// </summary>
    private const string ResourcePath = "Extension/agenix/message/processor";

    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(IMessageProcessor));

    /// <summary>
    ///     Type resolver used to locate and retrieve custom message processors by performing
    ///     resource path lookups, enabling dynamic discovery and management of processor types.
    /// </summary>
    private static readonly ResourcePathTypeResolver TypeResolver = new(ResourcePath);

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
    /// <param name="processor">The identifier of the message processor builder to retrieve.</param>
    /// <typeparam name="T">The type of IMessageProcessor.</typeparam>
    /// <typeparam name="TB">The type of the builder for the IMessageProcessor.</typeparam>
    /// <returns>An instance of the message processor builder, or null if it fails to resolve.</returns>
    public static Optional<IBuilder<T, TB>> Lookup<T, TB>(string processor)
        where T : IMessageProcessor where TB : IBuilder<T, TB>
    {
        try
        {
            return Optional<IBuilder<T, TB>>.Of(TypeResolver.Resolve<TB>(processor));
        }
        catch (AgenixSystemException)
        {
            Log.Warn($"Failed to resolve message processor from resource '{ResourcePath}/{processor}'");
        }

        return Optional<IBuilder<T, TB>>.Empty;
    }

    /// <summary>
    ///     Interface IBuilder defines the contract for building instances of
    ///     IMessageProcessor implementations.
    /// </summary>
    new interface IBuilder<out T, TB> where T : IMessageProcessor where TB : IBuilder<T, TB>
    {
        /// <summary>
        ///     Builds a new message processor instance.
        /// </summary>
        /// <returns></returns>
        T Build();
    }
}