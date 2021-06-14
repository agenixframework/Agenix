using System;
using System.Collections.Generic;
using FleetPay.Core.Exceptions;
using FleetPay.Core.Message;
using FleetPay.Core.Util;
using FleetPay.Core.Validation.Matcher;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace FleetPay.Core.Validation.Json
{
    public class JsonPathValidator : IJsonPathValidator
    {
        /// <summary>
        ///     Map defines json path expressions and target matcher names
        /// </summary>
        private readonly Dictionary<string, string> _jsonPathExpressions;

        public JsonPathValidator() : this(new Builder())
        {
        }

        /// <summary>
        ///     Constructor using fluent builder.
        /// </summary>
        /// <param name="builder"></param>
        private JsonPathValidator(Builder builder)
        {
            _jsonPathExpressions = builder.JsonPathExpressions;
        }

        public void Process(string payload, TestContext context)
        {
        }

        public void Validate(string payload, TestContext context)
        {
            if (_jsonPathExpressions == null || _jsonPathExpressions.Count == 0) return;

            if (string.IsNullOrEmpty(payload))
                throw new ValidationException(
                    "Unable to validate content elements - receive content payload was empty");

            Console.WriteLine("Start JSONPath element validation...");

            try
            {
                var readerContext = JToken.Parse(payload);


                foreach (var (key, value) in _jsonPathExpressions)
                {
                    var jsonPathExpression = context.ReplaceDynamicContentInString(key);
                    var jsonPathResult = JsonPathUtils.EvaluateAsString(readerContext, jsonPathExpression);
                    var expectedValueMatcher = context.ReplaceDynamicContentInString(value);

                    if (ValidationMatcherUtils.IsValidationMatcherExpression(expectedValueMatcher))
                    {
                        ValidationMatcherUtils.ResolveValidationMatcher(jsonPathExpression, jsonPathResult,
                            expectedValueMatcher, context);
                    }
                    else
                    {
                        if (!jsonPathResult.Equals(expectedValueMatcher))
                            throw new ValidationException(BuildValueMismatchErrorMessage(
                                "Values not equal for element '" + jsonPathExpression + "'", expectedValueMatcher,
                                jsonPathResult));
                    }

                    Console.WriteLine("Validating element: " + jsonPathExpression + "='" + expectedValueMatcher +
                                      "': OK.");
                }

                Console.WriteLine("JSONPath element validation successful: All values OK");
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
        public Dictionary<string, string> GetJsonPathExpressions()
        {
            return _jsonPathExpressions;
        }

        private static string BuildValueMismatchErrorMessage(string baseMessage, object controlValue,
            object actualValue)
        {
            return baseMessage + ", expected '" + controlValue + "' but was '" + actualValue + "'";
        }

        /// <summary>
        ///     Fluent builder.
        /// </summary>
        public sealed class Builder : IJsonPathValidator.IBuilder<JsonPathValidator, Builder>,
            IMessageProcessor.IBuilder<JsonPathValidator, Builder>
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

            public JsonPathValidator Build()
            {
                return new(this);
            }
        }
    }
}