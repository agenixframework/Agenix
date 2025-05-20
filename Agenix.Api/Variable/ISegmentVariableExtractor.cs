using Agenix.Api.Context;

namespace Agenix.Api.Variable;

/// <summary>
///     Class extracting values of segments of VariableExpressions.
/// </summary>
public interface ISegmentVariableExtractor
{
    /// <summary>
    ///     Extract variables from given object.
    /// </summary>
    /// <param name="testContext">the test context</param>
    /// <param name="obj">the object of which to extract the value</param>
    /// <param name="matcher"></param>
    bool CanExtract(TestContext testContext, object obj, VariableExpressionSegmentMatcher matcher);

    /// <summary>
    ///     Extract variables from a given object. Implementations should throw a CitrusRuntimeException
    /// </summary>
    /// <param name="testContext">the test context</param>
    /// <param name="obj">the object of which to extract the value</param>
    /// <param name="matcher"></param>
    object ExtractValue(TestContext testContext, object obj, VariableExpressionSegmentMatcher matcher);
}