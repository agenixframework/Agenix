using Agenix.Api.Context;
using Agenix.Api.Message;
using Agenix.Api.Variable;
using Agenix.Core.Variable;

namespace Agenix.Core.Message;

/// <summary>
///     Extracts variables from a message by delegating the extraction work to another specified variable extractor.
/// </summary>
/// <param name="variableExtractor">The variable extractor to which the extraction work is delegated.</param>
public class DelegatingVariableExtractor(VariableExtractor variableExtractor) : IVariableExtractor
{
    /// <summary>
    /// Extracts variables from a message and context using the provided variable extractor delegate.
    /// </summary>
    /// <param name="message">The message from which variables are to be extracted.</param>
    /// <param name="context">The context used during the variable extraction process.</param>
    public void ExtractVariables(IMessage message, TestContext context)
    {
        variableExtractor(message, context);
    }

    /// <summary>
    /// Processes the given message within the provided test context by extracting variables.
    /// </summary>
    /// <param name="message">The message to be processed.</param>
    /// <param name="context">The test context in which the message processing occurs.</param>
    public void Process(IMessage message, TestContext context)
    {
        ExtractVariables(message, context);
    }
}