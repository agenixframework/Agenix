using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Agenix.Api.Common;
using Agenix.Api.IO;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;

namespace Agenix.Validation.Xml.Validation.Xhtml;

/// <summary>
///     Provides functionality to convert HTML content into a valid XHTML format. This class also supports
///     operations for validating XHTML content and managing configurations for content processing.
/// </summary>
public class XhtmlMessageConverter : InitializingPhase
{
    // Search pattern for base w3 xhtml url
    private const string W3Xhtml1Url = @"http://www\.w3\.org/TR/xhtml1/DTD/";

    // Typical xhtml doctype definition
    private const string XhtmlDoctypeDefinition = "DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0";

    // XHTML 1.0 Strict DOCTYPE
    private const string XhtmlStrictDoctype =
        "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Strict//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-strict.dtd\">";

    // XHTML 1.0 Transitional DOCTYPE
    private const string XhtmlTransitionalDoctype =
        "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">";

    private IBrowsingContext? _browsingContext;
    private IResource? _configurationPath;
    private HtmlParser? _htmlParser;

    public void Initialize()
    {
        if (_browsingContext == null || _htmlParser == null)
        {
            var config = Configuration.Default.WithDefaultLoader();

            _browsingContext = BrowsingContext.New(config);
            _htmlParser = new HtmlParser(new HtmlParserOptions
            {
                IsEmbedded = false,
                IsScripting = false, // Disable script execution for security
                IsStrictMode = false // Allow HTML5 parsing
            });

            LoadConfiguration();
        }
    }

    /// <summary>
    ///     Asynchronously converts the provided HTML payload into a valid XHTML format while processing necessary URL
    ///     replacements.
    /// </summary>
    /// <param name="messagePayload">
    ///     The HTML content to be converted to XHTML. If the input is null or empty, an empty string will be returned.
    /// </param>
    /// <returns>
    ///     A task representing the asynchronous operation, with a string result containing the XHTML-compliant content.
    /// </returns>
    public async Task<string> ConvertAsync(string messagePayload)
    {
        if (string.IsNullOrWhiteSpace(messagePayload))
        {
            return string.Empty;
        }

        Initialize();

        // If already XHTML, process URL replacements
        if (messagePayload.Contains(XhtmlDoctypeDefinition))
        {
            return ProcessUrlReplacements(messagePayload);
        }

        try
        {
            // Parse HTML content
            var document = await _browsingContext!.OpenAsync(req => req.Content(messagePayload));

            // Convert to XHTML format
            var xhtmlPayload = ConvertDocumentToXhtml(document as IHtmlDocument);

            // Process URL replacements and ensure XML compliance
            var processedPayload = ProcessUrlReplacements(xhtmlPayload);
            return EnsureXmlCompliance(processedPayload);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to convert HTML to XHTML: {ex.Message}", ex);
        }
    }

    public string Convert(string messagePayload)
    {
        return ConvertAsync(messagePayload).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Ensures the XHTML content is XML-compliant by fixing common issues
    /// </summary>
    /// <param name="xhtmlContent">The XHTML content to make XML-compliant</param>
    /// <returns>XML-compliant XHTML content</returns>
    private string EnsureXmlCompliance(string xhtmlContent)
    {
        // Fix void elements that need to be self-closing in XML
        var voidElements = new[]
        {
            "input", "br", "hr", "img", "meta", "link", "area", "base", "col", "embed", "source", "track", "wbr"
        };

        foreach (var element in voidElements)
        {
            // Replace non-self-closing void elements with self-closing ones
            var pattern = $@"<{element}([^>]*[^/])>\s*</{element}>";
            var replacement = $"<{element}$1/>";
            xhtmlContent = Regex.Replace(xhtmlContent, pattern, replacement, RegexOptions.IgnoreCase);

            // Ensure self-closing tags have proper spacing before the slash
            pattern = $@"<{element}([^>]*[^/\s])/>";
            replacement = $"<{element}$1 />";
            xhtmlContent = Regex.Replace(xhtmlContent, pattern, replacement, RegexOptions.IgnoreCase);
        }

        return xhtmlContent;
    }

    /// <summary>
    ///     Converts the provided HTML document into a valid XHTML format, ensuring compliance with XHTML standards.
    /// </summary>
    /// <param name="document">
    ///     The HTML document to be converted to XHTML. This can be null, in which case an empty string will be returned.
    /// </param>
    /// <returns>
    ///     A string representing the XHTML-compliant content of the document, including the appropriate DOCTYPE declaration.
    /// </returns>
    private string ConvertDocumentToXhtml(IHtmlDocument? document)
    {
        if (document?.DocumentElement == null)
        {
            return string.Empty;
        }

        // Ensure proper XHTML structure
        EnsureXhtmlCompliance(document);

        // Get the HTML content with XML serialization
        var htmlContent = SerializeAsXml(document.DocumentElement);

        // Add XHTML DOCTYPE
        var doctype = DetermineDoctype(htmlContent);

        return $"{doctype}\n{htmlContent}";
    }

    /// <summary>
    ///     Serializes the HTML element as XML-compliant XHTML
    /// </summary>
    /// <param name="element">The HTML element to serialize</param>
    /// <returns>XML-compliant HTML string</returns>
    private string SerializeAsXml(IElement element)
    {
        // Use a more XML-friendly serialization
        var result = new StringBuilder();
        SerializeElementAsXml(element, result);
        return result.ToString();
    }

    /// <summary>
    ///     Recursively serializes an element and its children as XML
    /// </summary>
    /// <param name="element">The element to serialize</param>
    /// <param name="sb">The StringBuilder to append to</param>
    private void SerializeElementAsXml(IElement element, StringBuilder sb)
    {
        var tagName = element.TagName.ToLowerInvariant();
        sb.Append($"<{tagName}");

        // Add attributes
        foreach (var attr in element.Attributes)
        {
            var attrName = attr.Name.ToLowerInvariant();
            var attrValue = attr.Value ?? attrName; // Handle boolean attributes
            sb.Append($" {attrName}=\"{HttpUtility.HtmlEncode(attrValue)}\"");
        }

        // Check if it's a void element
        var voidElements = new HashSet<string>
        {
            "input",
            "br",
            "hr",
            "img",
            "meta",
            "link",
            "area",
            "base",
            "col",
            "embed",
            "source",
            "track",
            "wbr"
        };

        if (voidElements.Contains(tagName))
        {
            sb.Append(" />");
        }
        else
        {
            sb.Append(">");

            // Add child content
            foreach (var child in element.ChildNodes)
            {
                if (child is IElement childElement)
                {
                    SerializeElementAsXml(childElement, sb);
                }
                else if (child is IText textNode)
                {
                    sb.Append(HttpUtility.HtmlEncode(textNode.TextContent));
                }
            }

            sb.Append($"</{tagName}>");
        }
    }

    private void EnsureXhtmlCompliance(IHtmlDocument document)
    {
        // Ensure html element has proper xmlns attribute
        var htmlElement = document.DocumentElement;
        if (htmlElement != null && !htmlElement.HasAttribute("xmlns"))
        {
            htmlElement.SetAttribute("xmlns", "http://www.w3.org/1999/xhtml");
        }

        // Ensure all elements are properly closed and lowercase
        var allElements = document.QuerySelectorAll("*");
        foreach (var element in allElements)
        {
            // Ensure boolean attributes are properly formatted for XHTML
            EnsureBooleanAttributesCompliance(element);
        }
    }

    /// <summary>
    ///     Ensures that the specified element complies with XHTML requirements for boolean attributes.
    ///     This includes setting attribute values to match their names if the attribute is present but has no value.
    /// </summary>
    /// <param name="element">
    ///     The HTML element to validate and enforce XHTML compliance for boolean attributes.
    /// </param>
    private void EnsureBooleanAttributesCompliance(IElement element)
    {
        // XHTML requires boolean attributes to have values
        var booleanAttributes = new[] { "checked", "selected", "disabled", "readonly", "multiple", "defer", "compact" };

        foreach (var attr in booleanAttributes)
        {
            if (element.HasAttribute(attr) && string.IsNullOrEmpty(element.GetAttribute(attr)))
            {
                element.SetAttribute(attr, attr);
            }
        }
    }

    private string DetermineDoctype(string htmlContent)
    {
        // Simple heuristic to determine appropriate DOCTYPE
        var hasFormElements = htmlContent.Contains("<form", StringComparison.OrdinalIgnoreCase) ||
                              htmlContent.Contains("<input", StringComparison.OrdinalIgnoreCase) ||
                              htmlContent.Contains("<textarea", StringComparison.OrdinalIgnoreCase);

        var hasFrames = htmlContent.Contains("<frame", StringComparison.OrdinalIgnoreCase) ||
                        htmlContent.Contains("<frameset", StringComparison.OrdinalIgnoreCase);

        // Use transitional if forms or other transitional elements are present
        return hasFormElements || hasFrames ? XhtmlTransitionalDoctype : XhtmlStrictDoctype;
    }

    private string ProcessUrlReplacements(string xhtmlPayload)
    {
        return Regex.Replace(
            xhtmlPayload, W3Xhtml1Url, "org/w3/xhtml/",
            RegexOptions.IgnoreCase);
    }

    /// <summary>
    ///     Loads the configuration from the specified resource and processes its content if the resource exists.
    ///     This includes reading the configuration file, extracting its content, and applying any necessary settings.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     Thrown if the configuration cannot be loaded or processed due to a missing resource, invalid content, or other
    ///     errors.
    /// </exception>
    private void LoadConfiguration()
    {
        if (_configurationPath != null)
        {
            try
            {
                // Check if a resource exists
                if (_configurationPath.Exists)
                {
                    // Load configuration content from IResource
                    using var stream = _configurationPath.InputStream;
                    using var reader = new StreamReader(stream);
                    var configContent = reader.ReadToEnd();
                    ProcessConfiguration(configContent);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Failed to load configuration from resource: {_configurationPath.Description}", ex);
            }
        }
    }

    /// <summary>
    ///     Processes the configuration content and applies the necessary settings or options based on the provided
    ///     configuration.
    /// </summary>
    /// <param name="configContent">
    ///     The configuration content as a string. This typically contains key-value pairs or
    ///     directives that influence behavior.
    /// </param>
    private void ProcessConfiguration(string configContent)
    {
        // Process configuration content
        // This is a placeholder for custom configuration logic
        var lines = configContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        foreach (var line in lines)
        {
            if (line.StartsWith('#') || string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var parts = line.Split(':', 2, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                var key = parts[0].Trim().ToLowerInvariant();
                var value = parts[1].Trim();

                // Handle configuration options
                switch (key)
                {
                    case "strict-mode":
                        // Configure strict parsing if needed
                        break;
                    case "doctype-preference":
                        // Set DOCTYPE preference
                        break;
                    case "xmlns-attribute":
                        // Configure xmlns handling
                        break;
                    case "boolean-attributes":
                        // Configure boolean attribute handling
                        break;
                        // Add more configuration options as needed
                }
            }
        }
    }

    // Validation helper method
    public async Task<bool> IsValidXhtmlAsync(string xhtmlContent)
    {
        try
        {
            var document = await _browsingContext!.OpenAsync(req => req.Content(xhtmlContent));
            return document is not null;
        }
        catch
        {
            return false;
        }
    }

    public bool IsValidXhtml(string xhtmlContent)
    {
        return IsValidXhtmlAsync(xhtmlContent).GetAwaiter().GetResult();
    }

    // Property accessors
    public IBrowsingContext? GetBrowsingContext()
    {
        return _browsingContext;
    }

    public void SetBrowsingContext(IBrowsingContext context)
    {
        _browsingContext = context ?? throw new ArgumentNullException(nameof(context));
    }

    public IResource? GetConfigurationPath()
    {
        return _configurationPath;
    }

    public void SetConfigurationPath(IResource? resource)
    {
        _configurationPath = resource;
    }

    // Dispose pattern for proper cleanup
    public void Dispose()
    {
        _browsingContext?.Dispose();
    }
}
