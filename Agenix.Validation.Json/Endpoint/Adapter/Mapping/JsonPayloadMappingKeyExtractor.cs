using Agenix.Api.Message;
using Agenix.Core.Endpoint.Adapter.Mapping;
using Agenix.Validation.Json.Json;

namespace Agenix.Validation.Json.Endpoint.Adapter.Mapping;

/// <summary>
///     Extracts a mapping key from a JSON payload using a JSON Path expression.
/// </summary>
public class JsonPayloadMappingKeyExtractor : AbstractMappingKeyExtractor
{
    /// <summary>
    ///     JSON Path expression evaluated on message payload
    /// </summary>
    private string _jsonPathExpression = "$.KeySet()";

    /// <summary>
    ///     Evaluates and returns the mapping key from the provided request message's payload using a JSON Path expression.
    /// </summary>
    /// <param name="request">The message from which to extract the mapping key.</param>
    /// <returns>The extracted mapping key as a string.</returns>
    protected override string GetMappingKey(IMessage request)
    {
        return JsonPathUtils.EvaluateAsString(request.GetPayload<string>(), _jsonPathExpression);
    }

    /// <summary>
    ///     Sets the JSON Path expression to be evaluated on the message payload.
    /// </summary>
    /// <param name="jsonPathExpression">The JSON Path expression to use for extraction.</param>
    public void SetJsonPathExpression(string jsonPathExpression)
    {
        _jsonPathExpression = jsonPathExpression;
    }
}