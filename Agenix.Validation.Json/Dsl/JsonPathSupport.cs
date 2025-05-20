using Agenix.Api.Builder;
using Agenix.Api.Message;
using Agenix.Api.Validation.Context;
using Agenix.Api.Variable;
using Agenix.Core.Validation.Json;
using Agenix.Validation.Json.Validation;

namespace Agenix.Validation.Json.Dsl;

/// <summary>
///     Provides functionality to work with JSON paths in a Domain-Specific Language (DSL) manner.
/// </summary>
public class JsonPathSupport : IWithExpressions<JsonPathSupport>, IPathExpressionAdapter
{
    private readonly Dictionary<string, object> _expressions = new();

    /// <summary>
    ///     Transforms the current JsonPathSupport instance into an IMessageProcessor.
    /// </summary>
    /// <returns>Returns an instance of IMessageProcessor.</returns>
    public IMessageProcessor AsProcessor()
    {
        return new JsonPathMessageProcessor.Builder()
            .Expressions(_expressions)
            .Build();
    }

    /// <summary>
    ///     Converts the current JsonPathSupport instance to an IVariableExtractor.
    /// </summary>
    /// <returns>An instance of IVariableExtractor.</returns>
    public IVariableExtractor AsExtractor()
    {
        return new JsonPathVariableExtractor.Builder()
            .Expressions(_expressions)
            .Build();
    }

    /// <summary>
    ///     Converts the current instance to an <see cref="IValidationContext" /> implementation.
    /// </summary>
    /// <returns>An instance of <see cref="IValidationContext" />.</returns>
    public IValidationContext AsValidationContext()
    {
        return new JsonPathMessageValidationContext.Builder()
            .Expressions(_expressions)
            .Build();
    }

    /// <summary>
    ///     Adds multiple expressions to the JsonPathSupport instance.
    /// </summary>
    /// <param name="expressions">A dictionary containing expressions and their corresponding values.</param>
    /// <returns>The current JsonPathSupport instance with added expressions.</returns>
    public JsonPathSupport Expressions(IDictionary<string, object> expressions)
    {
        foreach (var expression in expressions) _expressions[expression.Key] = expression.Value;
        return this;
    }

    /// <summary>
    ///     Adds an expression and its associated value to the internal dictionary.
    /// </summary>
    /// <param name="expression">The expression to be added as a key.</param>
    /// <param name="value">The value associated with the expression.</param>
    /// <returns>The current instance of <see cref="JsonPathSupport" />.</returns>
    public JsonPathSupport Expression(string expression, object value)
    {
        _expressions[expression] = value;
        return this;
    }

    /// <summary>
    ///     Static entrance for all JsonPath related C# DSL functionalities.
    /// </summary>
    /// <returns></returns>
    public static JsonPathSupport JsonPath()
    {
        return new JsonPathSupport();
    }
}