using System.Collections.Generic;
using MPP.Core.Exceptions;
using MPP.Core.Message;
using MPP.Core.Variable;

namespace MPP.Core.Validation.Json
{
    /// <summary>
    ///     Variable extractor reading message headers and saves them to new test variables.
    /// </summary>
    public class HeaderVariableExtractor : IVariableExtractor
    {
        /// <summary>
        ///     Map holding header names and target variable names
        /// </summary>
        private readonly Dictionary<string, string> _headerMappings;

        public HeaderVariableExtractor() : this(new Builder())
        {
        }

        /// <summary>
        ///     Constructor using fluent builder.
        /// </summary>
        /// <param name="builder"></param>
        private HeaderVariableExtractor(Builder builder)
        {
            _headerMappings = builder.JsonPathExpressions;
        }


        public void Process(string payload, TestContext context)
        {
            ExtractVariables(payload, context);
        }

        /// <summary>
        ///     Reads header information and saves new test variables.
        /// </summary>
        /// <param name="payload"></param>
        /// <param name="context"></param>
        public void ExtractVariables(object payload, TestContext context)
        {
            if (_headerMappings == null || _headerMappings.Count == 0) return;

            var headers = (Dictionary<string, string>) payload;

            foreach (var (key, value) in _headerMappings)
            {
                if (!headers.ContainsKey(key.Trim()))
                    throw new UnknownElementException("Could not find header element " + key + " in received header");

                context.SetVariable(value, headers[key]);
            }
        }

        /// <summary>
        ///     Gets the headerMappings.
        /// </summary>
        /// <returns>the headerMappings</returns>
        public Dictionary<string, string> GetHeaderMappings()
        {
            return _headerMappings;
        }

        /// <summary>
        ///     Fluent builder
        /// </summary>
        public sealed class Builder : IVariableExtractor.IBuilder<HeaderVariableExtractor, Builder>,
            IMessageProcessor.IBuilder<HeaderVariableExtractor, Builder>
        {
            internal Dictionary<string, string> JsonPathExpressions = new();

            public Builder Expressions(Dictionary<string, string> expressions)
            {
                foreach (var (key, value) in expressions) JsonPathExpressions.Add(key, value);

                return this;
            }

            public Builder Expression(string headerName, string variableName)
            {
                JsonPathExpressions.Add(headerName, variableName);
                return this;
            }

            public HeaderVariableExtractor Build()
            {
                return new(this);
            }

            /// <summary>
            ///     Evaluate all header name expressions and store values as new variables to the test context.
            /// </summary>
            /// <param name="expressions"></param>
            /// <returns></returns>
            public Builder Headers(Dictionary<string, string> expressions)
            {
                foreach (var (key, value) in expressions) JsonPathExpressions.Add(key, value);

                return this;
            }

            /// <summary>
            ///     Reads header by its name and stores value as new variable to the test context.
            /// </summary>
            /// <param name="headerName"></param>
            /// <param name="variableName"></param>
            /// <returns></returns>
            public Builder Header(string headerName, string variableName)
            {
                JsonPathExpressions.Add(headerName, variableName);
                return this;
            }
        }
    }
}