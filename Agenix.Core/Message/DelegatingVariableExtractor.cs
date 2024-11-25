using Agenix.Core.Variable;

namespace Agenix.Core.Message;

/// <summary>
///     Extracts variables from a message by delegating the extraction work to another specified variable extractor.
/// </summary>
/// <param name="variableExtractor">The variable extractor to which the extraction work is delegated.</param>
public class DelegatingVariableExtractor(VariableExtractor variableExtractor) : IVariableExtractor
{
    public void ExtractVariables(IMessage message, TestContext context)
    {
        variableExtractor(message, context);
    }

    public void Process(IMessage message, TestContext context)
    {
        ExtractVariables(message, context);
    }
}