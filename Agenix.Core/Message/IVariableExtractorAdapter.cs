using Agenix.Core.Variable;

namespace Agenix.Core.Message;

// Delegate declaration for functional interface behavior
public delegate IVariableExtractor VariableExtractorAdapter();

/// <summary>
///     Adapter interface marks that a class is able to act as a variable extractor.
/// </summary>
public interface IVariableExtractorAdapter
{
    /// <summary>
    ///     Adapt as variable extractor
    /// </summary>
    /// <returns>A VariableExtractor instance</returns>
    IVariableExtractor AsExtractor();
}