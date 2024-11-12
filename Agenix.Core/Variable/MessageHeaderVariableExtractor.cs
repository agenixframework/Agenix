using System.Collections.Generic;
using Agenix.Core.Exceptions;
using Agenix.Core.Message;

namespace Agenix.Core.Variable;

/// <summary>
///     Variable extractor reading message headers and saves them to new test variables.
/// </summary>
public class MessageHeaderVariableExtractor : IVariableExtractor
{
    /// <summary>
    ///     Map holding header names and target variable names
    /// </summary>
    private readonly IDictionary<string, object> _headerMappings;

    public MessageHeaderVariableExtractor() : this(new Builder())
    {
    }

    /// <summary>
    ///     Constructor using fluent builder.
    /// </summary>
    /// <param name="builder"></param>
    private MessageHeaderVariableExtractor(Builder builder)
    {
        _headerMappings = builder.JsonPathExpressions;
    }

    /// <summary>
    ///     Processes the provided message and updates the test context with extracted variables.
    /// </summary>
    /// <param name="message">The message to process and extract variables from.</param>
    /// <param name="context">The test context where the extracted variables will be saved.</param>
    public void Process(IMessage message, TestContext context)
    {
        ExtractVariables(message, context);
    }

    /// <summary>
    ///     Reads header information and saves new test variables.
    /// </summary>
    /// <param name="message"></param>
    /// <param name="context"></param>
    public void ExtractVariables(IMessage message, TestContext context)
    {
        foreach (var (headerElementName, value) in _headerMappings)
        {
            var targetVariableName = value?.ToString()
                                     ?? throw new CoreSystemException(
                                         $"Variable name must be set for extractor on header '{headerElementName}'");

            if (message.GetHeader(headerElementName) == null)
                throw new UnknownElementException(
                    $"Could not find header element {headerElementName} in received header");

            context.SetVariable(targetVariableName, message.GetHeader(headerElementName).ToString());
        }
    }

    /// <summary>
    ///     Gets the headerMappings.
    /// </summary>
    /// <returns>the headerMappings</returns>
    public IDictionary<string, object> GetHeaderMappings()
    {
        return _headerMappings;
    }

    /// <summary>
    ///     Fluent builder
    /// </summary>
    public sealed class Builder : IVariableExtractor.IBuilder<MessageHeaderVariableExtractor, Builder>
    {
        internal readonly IDictionary<string, object> JsonPathExpressions = new Dictionary<string, object>();

        /// <summary>
        ///     Builds an instance of the MessageHeaderVariableExtractor class using the fluent builder pattern.
        /// </summary>
        /// <returns>
        ///     An instance of MessageHeaderVariableExtractor.
        /// </returns>
        public MessageHeaderVariableExtractor Build()
        {
            return new MessageHeaderVariableExtractor(this);
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

        /// <summary>
        ///     Static entry method for building a new instance of the Builder class.
        /// </summary>
        /// <returns>
        ///     A new Builder instance.
        /// </returns>
        public static Builder FromHeaders()
        {
            return new Builder();
        }

        /// <summary>
        ///     Evaluate all header name expressions and store values as new variables to the test context.
        /// </summary>
        /// <param name="expressions"></param>
        /// <returns></returns>
        public Builder Headers(IDictionary<string, string> expressions)
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