#region License
// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.
#endregion

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
