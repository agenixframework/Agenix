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

using Agenix.Api.Builder;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Agenix.Validation.Json.Validation;

/// <summary>
///     A processor that handles messages based on JSONPath expressions.
/// </summary>
/// <remarks>
///     JsonPathMessageProcessor processes messages by applying JSONPath expressions and updating the message payload
///     accordingly.
///     It also includes functionality to determine if a given message type is supported by this processor.
/// </remarks>
public class JsonPathMessageProcessor : AbstractMessageProcessor
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(JsonPathMessageProcessor));

    /// <summary>
    ///     Optional ignoring element not found errors
    /// </summary>
    private readonly bool _ignoreNotFound;

    /// <summary>
    ///     Overwrites message elements before validating (via JSONPath expressions)
    /// </summary>
    private readonly IDictionary<string, object> _jsonPathExpressions;

    /// <summary>
    ///     A processor that handles messages based on JSONPath expressions.
    /// </summary>
    public JsonPathMessageProcessor() : this(new Builder())
    {
    }

    private JsonPathMessageProcessor(Builder builder)
    {
        _jsonPathExpressions = builder._expressions;
        _ignoreNotFound = builder._ignoreNotFound;
    }

    /// <summary>
    ///     Gets the JSONPath expressions used for processing messages and updating message payloads.
    /// </summary>
    /// <remarks>
    ///     This property contains a collection of JSONPath expressions that are applied
    ///     to messages during processing, facilitating modifications or validations
    ///     based on the provided expressions.
    /// </remarks>
    public IDictionary<string, object> JsonPathExpressions => _jsonPathExpressions;

    /// <summary>
    ///     Processes the given message by applying JSONPath expressions and updating the message payload accordingly.
    /// </summary>
    /// <param name="message">The message to process.</param>
    /// <param name="context">The test context that provides dynamic content for JSONPath expressions.</param>
    public override void ProcessMessage(IMessage message, TestContext context)
    {
        if (string.IsNullOrEmpty(message.GetPayload<string>()))
        {
            return;
        }

        try
        {
            var jsonData = JToken.Parse(message.GetPayload<string>());

            foreach (var entry in _jsonPathExpressions)
            {
                var jsonPathExpression = entry.Key;
                var valueExpression = context.ReplaceDynamicContentInString(entry.Value.ToString());

                object value;
                switch (valueExpression)
                {
                    case "true":
                        value = true;
                        break;
                    case "false":
                        value = false;
                        break;
                    default:
                        {
                            if (!int.TryParse(valueExpression, out var intValue))
                            {
                                value = valueExpression;
                            }
                            else
                            {
                                value = intValue;
                            }

                            break;
                        }
                }

                try
                {
                    SetJsonValue(jsonData, jsonPathExpression, value);
                }
                catch (JsonException ex)
                {
                    if (!_ignoreNotFound)
                    {
                        throw new UnknownElementException(
                            $"Could not find element for expression: {jsonPathExpression}", ex);
                    }
                }

                if (Log.IsEnabled(LogLevel.Debug))
                {
                    Log.LogDebug($"Element {jsonPathExpression} was set to value: {valueExpression}");
                }
            }

            message.Payload = jsonData.ToString(Formatting.None);
        }
        catch (JsonReaderException ex)
        {
            throw new AgenixSystemException("Failed to parse JSON text", ex);
        }
    }

    /// <summary>
    ///     Sets the value of a JSON token specified by a JSONPath expression.
    /// </summary>
    /// <param name="json">The JSON object to modify.</param>
    /// <param name="jsonPath">The JSONPath expression indicating the token to modify.</param>
    /// <param name="value">The new value to set for the specified token.</param>
    /// <exception cref="JsonException">Thrown if the JSONPath expression is not found or the token type is unsupported.</exception>
    private static void SetJsonValue(JToken json, string jsonPath, object value)
    {
        var tokens = json.SelectTokens(jsonPath);
        var pathFound = false;

        foreach (var token in tokens)
        {
            pathFound = true;

            switch (token)
            {
                case JValue jValue:
                    jValue.Value = value;
                    break;
                case JArray jArray:
                case JObject jObject:
                    token.Replace(JToken.FromObject(value));
                    break;
                default:
                    throw new JsonException($"Unsupported token type for path: {jsonPath}");
            }
        }

        if (!pathFound)
        {
            throw new JsonException($"Path not found: {jsonPath}");
        }
    }

    /// <summary>
    ///     Determines if this processor supports the given message type.
    /// </summary>
    /// <param name="messageType">The type of the message to check.</param>
    /// <returns>Returns true if the message type is JSON, otherwise false.</returns>
    public override bool SupportsMessageType(string messageType)
    {
        return nameof(MessageType.JSON).Equals(messageType, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    ///     The Builder class for constructing instances of the JsonPathMessageProcessor.
    /// </summary>
    /// <remarks>
    ///     The Builder class allows the configuration and creation of JsonPathMessageProcessor
    ///     instances. It supports configuring expressions and options, such as ignoring or handling
    ///     not found conditions during message processing.
    /// </remarks>
    public class Builder : IMessageProcessor.IBuilder<JsonPathMessageProcessor, Builder>, IWithExpressions<Builder>
    {
        internal readonly IDictionary<string, object> _expressions = new Dictionary<string, object>();
        internal bool _ignoreNotFound;

        /// <summary>
        ///     Constructs an instance of JsonPathMessageProcessor using the provided builder configuration.
        /// </summary>
        /// <returns>Returns an instance of JsonPathMessageProcessor.</returns>
        public JsonPathMessageProcessor Build()
        {
            return new JsonPathMessageProcessor(this);
        }

        /// <summary>
        ///     Adds multiple expressions to the builder for message processing.
        /// </summary>
        /// <param name="expressions">A dictionary containing expressions and their corresponding expected values.</param>
        /// <returns>The current builder instance with the added expressions.</returns>
        public Builder Expressions(IDictionary<string, object> expressions)
        {
            foreach (var expression in expressions)
            {
                _expressions[expression.Key] = expression.Value;
            }

            return this;
        }

        /// <summary>
        ///     Adds an expression and its expected value to the builder.
        /// </summary>
        /// <param name="expression">The expression to be added.</param>
        /// <param name="expectedValue">The expected value associated with the expression.</param>
        /// <returns>The Builder instance with the added expression.</returns>
        public Builder Expression(string expression, object expectedValue)
        {
            _expressions[expression] = expectedValue;
            return this;
        }

        /// <summary>
        ///     Configures whether message processing should ignore conditions where messages are not found.
        /// </summary>
        /// <param name="ignore">A boolean value indicating whether to ignore not found conditions.</param>
        /// <returns>Returns the current Builder instance for method chaining.</returns>
        public Builder IgnoreNotFound(bool ignore)
        {
            _ignoreNotFound = ignore;
            return this;
        }
    }
}
