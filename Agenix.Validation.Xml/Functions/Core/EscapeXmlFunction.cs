using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Functions;

namespace Agenix.Validation.Xml.Functions.Core;

/// <summary>
/// Escapes XML fragment with escaped characters for '&lt;', '&gt;'.
/// </summary>
public class EscapeXmlFunction : IFunction
{
    public string Execute(List<string> parameterList, TestContext context)
    {
        if (parameterList is not { Count: 1 })
        {
            throw new InvalidFunctionUsageException($"Invalid function parameter usage! Expected single parameter but found: {parameterList?.Count ?? 0}");
        }

        return EscapeXml(parameterList[0]);
    }

    /// <summary>
    /// Escapes XML special characters in the input string.
    /// </summary>
    /// <param name="input">The string to escape</param>
    /// <returns>XML-escaped string</returns>
    private static string EscapeXml(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return input;
        }

        return input
            .Replace("&", "&amp;")   // Must be first to avoid double-escaping
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .Replace("'", "&apos;");
    }
}
