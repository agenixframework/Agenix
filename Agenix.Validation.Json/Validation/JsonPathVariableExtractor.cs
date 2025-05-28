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

using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Agenix.Api.Variable;
using Agenix.Core.Message;
using Agenix.Core.Validation;
using Agenix.Validation.Json.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Agenix.Validation.Json.Validation;

/// <summary>
///     Extractor implementation reads message elements via JSONPath expressions and saves the
///     values as new test variables. JObject and JArray items will be saved as string representation.
/// </summary>
public class JsonPathVariableExtractor : IVariableExtractor
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(JsonPathMessageValidator));

    /// <summary>
    ///     Map defines JSON path expressions and target variable names
    /// </summary>
    private readonly IDictionary<string, object> _jsonPathExpressions;

    public JsonPathVariableExtractor() : this(new Builder())
    {
    }

    /// <summary>
    ///     Constructor using fluent builder.
    /// </summary>
    /// <param name="builder"></param>
    private JsonPathVariableExtractor(Builder builder)
    {
        _jsonPathExpressions = builder.JsonPathExpressions;
    }

    /// <summary>
    ///     Gets the JSONPath expressions.
    /// </summary>
    /// <returns></returns>
    public IDictionary<string, object> JsonPathExpressions => _jsonPathExpressions;

    /// <summary>
    ///     Processes the given message within the provided test context.
    /// </summary>
    /// <param name="message">The message object containing data to be processed.</param>
    /// <param name="context">The context in which the message will be processed.</param>
    public void Process(IMessage message, TestContext context)
    {
        ExtractVariables(message, context);
    }

    /// <summary>
    ///     Extracts variables from a JSON message using JSONPath expressions.
    /// </summary>
    /// <param name="receivedMessage">The received message from which variables are to be extracted.</param>
    /// <param name="context">The context in which the variables are evaluated and stored.</param>
    public void ExtractVariables(IMessage receivedMessage, TestContext context)
    {
        if (_jsonPathExpressions == null || _jsonPathExpressions.Count == 0) return;


        if (Log.IsEnabled(LogLevel.Debug)) Log.LogDebug("Reading JSON elements with JSONPath");

        try
        {
            var readerContext = JToken.Parse(receivedMessage.GetPayload<string>());

            foreach (var (key, value) in _jsonPathExpressions)
            {
                var jsonPathExpression = context.ReplaceDynamicContentInString(key);

                var variableName = value?.ToString()
                                   ?? throw new AgenixSystemException(
                                       $"Variable name must be set on extractor path expression '{jsonPathExpression}'");

                if (Log.IsEnabled(LogLevel.Debug)) Log.LogDebug("Evaluating JSONPath expression: " + jsonPathExpression);

                var jsonPathResult = JsonPathUtils.EvaluateAsString(readerContext, jsonPathExpression);
                context.SetVariable(variableName, jsonPathResult);
            }
        }
        catch (JsonReaderException e)
        {
            throw new AgenixSystemException("Failed to parse JSON text", e);
        }
    }

    /// <summary>
    ///     Fluent builder.
    /// </summary>
    public sealed class Builder : IVariableExtractor.IBuilder<JsonPathVariableExtractor, Builder>,
        IMessageProcessorAdapter, IValidationContextAdapter
    {
        internal readonly IDictionary<string, object> JsonPathExpressions = new Dictionary<string, object>();

        public JsonPathVariableExtractor Build()
        {
            return new JsonPathVariableExtractor(this);
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="Builder" /> class with the provided expressions.
        /// </summary>
        /// <param name="expressions">A dictionary containing the expressions to be added to the builder.</param>
        public Builder Expressions(IDictionary<string, object> expressions)
        {
            foreach (var (key, value) in expressions) JsonPathExpressions.Add(key, value);

            return this;
        }

        /// <summary>
        ///     Adds an expression and its associated value to the builder.
        /// </summary>
        /// <param name="expression">The string representation of the expression to be added.</param>
        /// <param name="value">The value associated with the expression.</param>
        /// <returns>The current instance of the builder.</returns>
        public Builder Expression(string expression, object value)
        {
            JsonPathExpressions.Add(expression, value);
            return this;
        }

        /// <summary>
        ///     Creates and returns an instance of <see cref="IMessageProcessor" />
        ///     configured with the specified JSON path expressions.
        /// </summary>
        /// <returns>
        ///     An instance of <see cref="IMessageProcessor" /> configured
        ///     for processing messages based on JSON path expressions.
        /// </returns>
        public IMessageProcessor AsProcessor()
        {
            return new DelegatingPathExpressionProcessor.Builder()
                .Expressions(JsonPathExpressions)
                .Build();
        }

        /// <summary>
        ///     Converts the current context through the fluent builder into an instance of <see cref="IValidationContext" />.
        /// </summary>
        /// <returns>An instance of <see cref="IValidationContext" /> built using configured expressions.</returns>
        public IValidationContext AsValidationContext()
        {
            return new PathExpressionValidationContext.Builder()
                .Expressions(JsonPathExpressions)
                .Build();
        }
    }
}
