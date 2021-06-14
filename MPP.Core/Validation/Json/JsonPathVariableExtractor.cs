using System.Collections.Generic;
using FleetPay.Core.Exceptions;
using FleetPay.Core.Message;
using FleetPay.Core.Util;
using FleetPay.Core.Variable;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FleetPay.Core.Validation.Json
{
    /// <summary>
    ///     Extractor implementation reads message elements via JSONPath expressions and saves the
    ///     values as new test variables. JObject and JArray items will be saved as string representation.
    /// </summary>
    public class JsonPathVariableExtractor : IVariableExtractor
    {
        /// <summary>
        ///     Map defines json path expressions and target variable names
        /// </summary>
        private readonly Dictionary<string, string> _jsonPathExpressions;

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

        public void ExtractVariables(object payload, TestContext context)
        {
            if (_jsonPathExpressions == null || _jsonPathExpressions.Count == 0) return;

            try
            {
                var readerContext = JToken.Parse(payload as string ?? string.Empty);

                foreach (var (key, value) in _jsonPathExpressions)
                {
                    var jsonPathExpression = context.ReplaceDynamicContentInString(key);

                    var jsonPathResult = JsonPathUtils.EvaluateAsString(readerContext, jsonPathExpression);
                    context.SetVariable(value, jsonPathResult);
                }
            }
            catch (JsonReaderException e)
            {
                throw new CoreSystemException("Failed to parse JSON text", e);
            }
        }

        public void Process(string payload, TestContext context)
        {
        }

        /// <summary>
        ///     Gets the JSONPath expressions.
        /// </summary>
        /// <returns></returns>
        public Dictionary<string, string> GetJsonPathExpressions()
        {
            return _jsonPathExpressions;
        }

        /// <summary>
        ///     Fluent builder.
        /// </summary>
        public sealed class Builder : IVariableExtractor.IBuilder<JsonPathVariableExtractor, Builder>,
            IMessageProcessor.IBuilder<JsonPathVariableExtractor, Builder>
        {
            internal Dictionary<string, string> JsonPathExpressions = new();

            public Builder Expressions(Dictionary<string, string> expressions)
            {
                foreach (var (key, value) in expressions) JsonPathExpressions.Add(key, value);

                return this;
            }

            public Builder Expression(string expression, string variableName)
            {
                JsonPathExpressions.Add(expression, variableName);
                return this;
            }

            public JsonPathVariableExtractor Build()
            {
                return new(this);
            }
        }
    }
}