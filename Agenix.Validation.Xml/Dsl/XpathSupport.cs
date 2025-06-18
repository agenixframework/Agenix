using Agenix.Api.Builder;
using Agenix.Api.Message;
using Agenix.Api.Validation.Context;
using Agenix.Api.Variable;
using Agenix.Core.Validation.Xml;
using Agenix.Validation.Xml.Validation.Xml;

namespace Agenix.Validation.Xml.Dsl;

/// <summary>
///     Provides support for configuring and using XPath-related functionalities within the DSL.
///     This class serves as a bridge to configure expressions and adapt to various
///     processing contexts such as message processing, variable extraction, and validation.
/// </summary>
public class XpathSupport : IWithExpressions<XpathSupport>, IPathExpressionAdapter
{
    private readonly Dictionary<string, object> _expressions = new();

    public IMessageProcessor AsProcessor()
    {
        return new XpathMessageProcessor.Builder()
            .Expressions(_expressions)
            .Build();
    }

    public IVariableExtractor AsExtractor()
    {
        return new XpathPayloadVariableExtractor.Builder()
            .Expressions(_expressions)
            .Build();
    }

    public IValidationContext AsValidationContext()
    {
        return new XpathMessageValidationContext.Builder()
            .Expressions(_expressions)
            .Build();
    }

    public XpathSupport Expressions(IDictionary<string, object> expressions)
    {
        foreach (var kvp in expressions)
        {
            _expressions[kvp.Key] = kvp.Value;
        }

        return this;
    }

    public XpathSupport Expression(string expression, object value)
    {
        _expressions[expression] = value;
        return this;
    }

    /// <summary>
    ///     Static entrance for all XPath-related C# DSL functionalities.
    /// </summary>
    /// <returns>New XpathSupport instance</returns>
    public static XpathSupport Xpath()
    {
        return new XpathSupport();
    }
}
