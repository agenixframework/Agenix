using System;
using System.Collections.Generic;
using Agenix.Core.Exceptions;
using Agenix.Core.Message;
using Agenix.Core.Util;
using Agenix.Core.Validation.Context;
using Agenix.Core.Variable;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Agenix.Core.Validation.Json;

/// <summary>
///     Extractor implementation reads message elements via JSONPath expressions and saves the
///     values as new test variables. JObject and JArray items will be saved as string representation.
/// </summary>
public class JsonPathVariableExtractor : IVariableExtractor
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILog _log = LogManager.GetLogger(typeof(JsonPathMessageValidator));

    /// <summary>
    ///     Map defines json path expressions and target variable names
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


        if (_log.IsDebugEnabled) _log.Debug("Reading JSON elements with JSONPath");

        try
        {
            var readerContext = JToken.Parse(receivedMessage.GetPayload<string>());

            foreach (var (key, value) in _jsonPathExpressions)
            {
                var jsonPathExpression = context.ReplaceDynamicContentInString(key);

                var variableName = value?.ToString()
                                   ?? throw new CoreSystemException(
                                       $"Variable name must be set on extractor path expression '{jsonPathExpression}'");

                if (_log.IsDebugEnabled) _log.Debug("Evaluating JSONPath expression: " + jsonPathExpression);

                var jsonPathResult = JsonPathUtils.EvaluateAsString(readerContext, jsonPathExpression);
                context.SetVariable(variableName, jsonPathResult);
            }
        }
        catch (JsonReaderException e)
        {
            throw new CoreSystemException("Failed to parse JSON text", e);
        }
    }

    /// <summary>
    ///     Gets the JSONPath expressions.
    /// </summary>
    /// <returns></returns>
    public IDictionary<string, object> GetJsonPathExpressions()
    {
        return _jsonPathExpressions;
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

        public Builder Expressions(IDictionary<string, object> expressions)
        {
            foreach (var (key, value) in expressions) JsonPathExpressions.Add(key, value);

            return this;
        }

        public Builder Expression(string expression, object value)
        {
            JsonPathExpressions.Add(expression, value);
            return this;
        }

        public IMessageProcessor AsProcessor()
        {
            throw new NotImplementedException();
        }

        public IValidationContext AsValidationContext()
        {
            throw new NotImplementedException();
        }
    }
}