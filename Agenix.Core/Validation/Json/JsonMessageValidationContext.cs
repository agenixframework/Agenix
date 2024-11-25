using System.Collections.Generic;
using Agenix.Core.Validation.Context;

namespace Agenix.Core.Validation.Json;

/// <summary>
///     Validation context holding JSON specific validation information.
/// </summary>
public class JsonMessageValidationContext : DefaultValidationContext
{
    /// <summary>
    ///     Set holding XPath expressions to identify the ignored message elements
    /// </summary>
    private readonly HashSet<string> _ignoreExpressions;

    // Default constructor
    public JsonMessageValidationContext() : this(new Builder())
    {
    }

    /**
     * Constructor using fluent builder.
     * @param builder
     */
    public JsonMessageValidationContext(Builder builder)
    {
        _ignoreExpressions = builder.IgnoreExpressions;
    }

    /// <summary>
    ///     Set holding XPath expressions to identify the ignored message elements
    /// </summary>
    public HashSet<string> IgnoreExpressions => _ignoreExpressions;

    public sealed class Builder : IValidationContext.IBuilder<JsonMessageValidationContext, Builder>
    {
        internal readonly HashSet<string> IgnoreExpressions = [];

        /// <summary>
        ///     Creates a new instance of JsonMessageValidationContext using the builder's current state.
        /// </summary>
        /// <returns>A JsonMessageValidationContext instance.</returns>
        public JsonMessageValidationContext Build()
        {
            return new JsonMessageValidationContext(this);
        }

        /// <summary>
        ///     Creates a new instance of JsonMessageValidationContext.Builder.
        /// </summary>
        /// <returns>A builder for creating a JsonMessageValidationContext.</returns>
        public static Builder Json()
        {
            return new Builder();
        }

        /// <summary>
        ///     Adds a JSON path expression to the validation context builder.
        /// </summary>
        /// <returns>A new instance of JsonPathMessageValidationContext.Builder initialized for JSON path message validation.</returns>
        public JsonPathMessageValidationContext.Builder Expressions()
        {
            return new JsonPathMessageValidationContext.Builder();
        }

        /// <summary>
        ///     Adds a JSON path expression to the validation context.
        /// </summary>
        /// <param name="path">The JSON path expression.</param>
        /// <param name="expectedValue">The expected value at the given JSON path.</param>
        /// <returns>A builder for chaining method calls.</returns>
        public JsonPathMessageValidationContext.Builder Expression(string path, object expectedValue)
        {
            return new JsonPathMessageValidationContext.Builder().Expression(path, expectedValue);
        }

        /// <summary>
        ///     Adds an ignore path expression for a message element.
        /// </summary>
        /// <param name="path">The path expression to be ignored.</param>
        /// <returns>A builder for chaining method calls.</returns>
        public Builder Ignore(string path)
        {
            IgnoreExpressions.Add(path);
            return this;
        }

        /// <summary>
        ///     Adds a JSON path to be ignored during validation.
        /// </summary>
        /// <param name="paths">The JSON path to be ignored</param>
        /// <returns>A builder for chaining method calls.</returns>
        public Builder Ignore(List<string> paths)
        {
            if (paths == null) return this;
            foreach (var path in paths) IgnoreExpressions.Add(path);
            return this;
        }
    }
}