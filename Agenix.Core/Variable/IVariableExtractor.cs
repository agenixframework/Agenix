using System;
using Agenix.Core.Builder;
using Agenix.Core.Exceptions;
using Agenix.Core.Message;
using Agenix.Core.Util;
using Agenix.Core.Validation.Json;
using log4net;

namespace Agenix.Core.Variable;

public delegate void VariableExtractor(IMessage message, TestContext context);

/// <summary>
///     Defines a contract for extracting variables from messages and updating test contexts accordingly.
/// </summary>
public interface IVariableExtractor : IMessageProcessor
{
    /// <summary>
    ///     A logger instance used for logging within the IVariableExtractor interface.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(IVariableExtractor));

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
    ///     Looks up and returns an optional builder for the specified IVariableExtractor type.
    /// </summary>
    /// <param name="validator">The type of IVariableExtractor to look up, such as "jsonPath".</param>
    /// <typeparam name="T">The IVariableExtractor implementation type.</typeparam>
    /// <typeparam name="TB">The builder type for the IVariableExtractor implementation.</typeparam>
    /// <returns>An optional builder for the specified IVariableExtractor type.</returns>
    public new static Optional<IBuilder<T, TB>> Lookup<T, TB>(string validator)
        where T : IVariableExtractor where TB : IBuilder<T, TB>
    {
        try
        {
            switch (validator)
            {
                case "jsonPath":
                {
                    var instance = (TB)Activator.CreateInstance(typeof(JsonPathVariableExtractor.Builder))!;
                    return Optional<IBuilder<T, TB>>.OfNullable(instance);
                }
            }
        }
        catch (CoreSystemException)
        {
            Log.Warn($"Failed to resolve message processor from resource '{validator}'");
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