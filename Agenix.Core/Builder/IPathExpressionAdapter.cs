using Agenix.Core.Message;
using Agenix.Core.Validation;

namespace Agenix.Core.Builder;

/// <summary>
///     Represents an adapter that consolidates the capabilities of processing messages, extracting variables, and
///     functioning within a validation context.
/// </summary>
public interface IPathExpressionAdapter : IMessageProcessorAdapter, IVariableExtractorAdapter, IValidationContextAdapter
{
}