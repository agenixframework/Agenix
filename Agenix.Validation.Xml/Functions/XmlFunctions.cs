using Agenix.Api.Context;
using Agenix.Validation.Xml.Functions.Core;

namespace Agenix.Validation.Xml.Functions;

/// <summary>
///     Utility class providing static methods for XML-related functions.
/// </summary>
public static class XmlFunctions
{
    /// <summary>
    ///     Runs create CData section function with arguments.
    /// </summary>
    /// <param name="content">The content to wrap in CDATA</param>
    /// <param name="context">The test context</param>
    /// <returns>Content wrapped in CDATA section</returns>
    public static string CreateCDataSection(string content, TestContext context)
    {
        return new CreateCDataSectionFunction().Execute([content], context);
    }

    /// <summary>
    ///     Runs escape XML function with arguments.
    /// </summary>
    /// <param name="content">The content to escape</param>
    /// <param name="context">The test context</param>
    /// <returns>XML-escaped content</returns>
    public static string EscapeXml(string content, TestContext context)
    {
        return new EscapeXmlFunction().Execute([content], context);
    }

    /// <summary>
    ///     Runs XPath function with arguments.
    /// </summary>
    /// <param name="content">The XML content to evaluate</param>
    /// <param name="expression">The XPath expression</param>
    /// <param name="context">The test context</param>
    /// <returns>Result of XPath evaluation</returns>
    public static string XPath(string content, string expression, TestContext context)
    {
        return new XpathFunction().Execute([content, expression], context);
    }
}
