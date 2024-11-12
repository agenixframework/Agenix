using System;
using System.Collections.Generic;
using Agenix.Core.Builder;
using Agenix.Core.Message;
using Agenix.Core.Validation.Context;
using Agenix.Core.Variable;

namespace Agenix.Core.Validation.Json;

/// <summary>
///     Specialised validation context adds JSON path expressions for message validation.
/// </summary>
public class JsonPathMessageValidationContext(JsonPathMessageValidationContext.Builder builder)
    : DefaultValidationContext
{
    // Map holding jsonPath expressions as key and expected values as values
    private readonly Dictionary<string, object> _jsonPathExpressions = builder._expressions;

    // Constructor using Fluent Builder

    // Default constructor
    public JsonPathMessageValidationContext() : this(new Builder())
    {
    }

    // Get the control message elements that have to be present in
    // the received message. Message element values are compared as well.
    public Dictionary<string, object> GetJsonPathExpressions()
    {
        return _jsonPathExpressions;
    }

    // Check whether the given path expression is a JSONPath expression
    public static bool IsJsonPathExpression(string pathExpression)
    {
        return !string.IsNullOrEmpty(pathExpression) && pathExpression.StartsWith('$');
    }

    // Fluent builder
    public class Builder : IValidationContext.IBuilder<JsonPathMessageValidationContext, Builder>,
        IWithExpressions<Builder>, IVariableExtractorAdapter, IMessageProcessorAdapter
    {
        internal readonly Dictionary<string, object> _expressions = new();

        public JsonPathMessageValidationContext Build()
        {
            return new JsonPathMessageValidationContext(this);
        }

        public IMessageProcessor AsProcessor()
        {
            throw new NotImplementedException("AsProcessor");
        }

        public IVariableExtractor AsExtractor()
        {
            throw new NotImplementedException("AsExtractor");
        }

        public Builder Expressions(IDictionary<string, object> expressions)
        {
            foreach (var kvp in expressions) _expressions[kvp.Key] = kvp.Value;
            return this;
        }

        public Builder Expression(string expression, object value)
        {
            _expressions[expression] = value;
            return this;
        }

        // Static entry method for fluent builder API
        public static Builder JsonPath()
        {
            return new Builder();
        }
    }
}