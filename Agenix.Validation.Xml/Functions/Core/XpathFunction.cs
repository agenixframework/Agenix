using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Functions;
using Agenix.Api.Xml.Namespace;
using Agenix.Validation.Xml.Util;
using Agenix.Validation.Xml.Xpath;

namespace Agenix.Validation.Xml.Functions.Core;

/// <summary>
/// XPath function for evaluating XPath expressions against XML sources.
/// </summary>
public class XpathFunction : IFunction
{
    public string Execute(List<string> parameterList, TestContext context)
    {
        if (parameterList == null || parameterList.Count == 0)
        {
            throw new InvalidFunctionUsageException("Function parameters must not be empty");
        }

        if (parameterList.Count < 2)
        {
            throw new InvalidFunctionUsageException("Missing parameter for function - usage xpath('xmlSource', 'expression')");
        }

        var xmlSource = parameterList[0];
        var xpathExpression = parameterList[1];

        var namespaceContext = new DefaultNamespaceContext();
        namespaceContext.AddNamespaces(context.NamespaceContextBuilder.NamespaceMappings);

        return XpathUtils.EvaluateAsString(
            XmlUtils.ParseMessagePayload(context.ReplaceDynamicContentInString(xmlSource)),
            context.ReplaceDynamicContentInString(xpathExpression),
            namespaceContext);
    }
}
