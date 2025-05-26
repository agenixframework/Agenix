using Agenix.Api.Builder;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Util;
using Agenix.Core.Spi;
using Microsoft.Extensions.Logging;

namespace Agenix.Api.Variable;

public delegate void VariableExtractor(IMessage message, TestContext context);

/// <summary>
///     Defines a contract for extracting variables from messages and updating test contexts accordingly.
/// </summary>
public interface IVariableExtractor : IMessageProcessor
{
    /// <summary>
    ///     Represents the resource lookup path used within the variable extractor operations.
    /// </summary>
    private const string ResourcePath = "Extension/agenix/variable/extractor";

    /// <summary>
    ///     A logger instance used for logging within the IVariableExtractor interface.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(IVariableExtractor));

    /// <summary>
    ///     A type resolver that dynamically identifies and locates custom variable extractors
    ///     using a specific resource path within the system.
    /// </summary>
    private static readonly ResourcePathTypeResolver TypeResolver = new(ResourcePath);

    /// <summary>
    ///     Processes the given message and updates the test context accordingly.
    /// </summary>
    /// <param name="message">The message to be processed.</param>
    /// <param name="context">The test context to be updated with the processed message data.</param>
    new void Process(IMessage message, TestContext context)
    {
        ExtractVariables(message, context);
    }

    /// <summary>
    ///     Extracts variables from the given message and adds them to the test context.
    /// </summary>
    /// <param name="message">The message from which variables are to be extracted.</param>
    /// <param name="context">The context to which the extracted variables will be added.</param>
    void ExtractVariables(IMessage message, TestContext context);

    /// <summary>
    ///     Looks up and returns an optional builder for the specified IVariableExtractor type.`
    /// </summary>
    /// <param name="extractor">The type of IVariableExtractor to look up, such as "jsonPath".</param>
    /// <typeparam name="T">The IVariableExtractor implementation type.</typeparam>
    /// <typeparam name="TB">The builder type for the IVariableExtractor implementation.</typeparam>
    /// <returns>An optional builder for the specified IVariableExtractor type.</returns>
    public new static Optional<IBuilder<T, TB>> Lookup<T, TB>(string extractor)
        where T : IVariableExtractor where TB : IBuilder<T, TB>
    {
        try
        {
            return Optional<IBuilder<T, TB>>.OfNullable(TypeResolver.Resolve<IBuilder<T, TB>>(extractor));
        }
        catch (AgenixSystemException)
        {
            Log.LogWarning(
                "Failed to resolve variable extractor from resource '{ExtensionAgenixVariableExtractor}/{Extractor}'",
                ResourcePath, extractor);
        }

        return Optional<IBuilder<T, TB>>.Empty;
    }

    /// <summary>
    ///     Provides a contract for building instances of implementations that adhere to the IVariableExtractor and
    ///     IMessageProcessor interfaces.
    /// </summary>
    /// <typeparam name="T">The type of the IVariableExtractor implementation being built.</typeparam>
    /// <typeparam name="TB">The type of the builder itself, implementing IMessageProcessor.IBuilder.</typeparam>
    public new interface IBuilder<out T, TB> : IMessageProcessor.IBuilder<T, TB>, IWithExpressions<TB>
        where T : IVariableExtractor
        where TB : IBuilder<T, TB>
    {
        new T Build();
    }
}