using Agenix.Core.Builder;
using Agenix.Core.Message;

namespace Agenix.Core.Variable;

/// <summary>
///     Defines a contract for extracting variables from messages and updating test contexts accordingly.
/// </summary>
public interface IVariableExtractor : IMessageProcessor
{
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
    ///     Provides a contract for building instances of implementations that adhere to the IVariableExtractor and
    ///     IMessageProcessor interfaces.
    /// </summary>
    /// <typeparam name="T">The type of the IVariableExtractor implementation being built.</typeparam>
    /// <typeparam name="TB">The type of the builder itself, implementing IMessageProcessor.IBuilder.</typeparam>
    public interface IBuilder<out T, TB> : IMessageProcessor.IBuilder<T, TB>, IWithExpressions<TB>
        where T : IVariableExtractor
        where TB : IBuilder<T, TB>
    {
        new T Build();
    }
}