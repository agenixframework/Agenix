using Agenix.Api.Message;
using Agenix.Api.Validation;
using Agenix.Core.Message;

namespace Agenix.Api.Builder;

/// <summary>
///     Represents an adapter that consolidates the capabilities of processing messages, extracting variables, and
///     functioning within a validation context.
/// </summary>
public interface IPathExpressionAdapter : IMessageProcessorAdapter, IVariableExtractorAdapter, IValidationContextAdapter
{
}