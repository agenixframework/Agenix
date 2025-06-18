using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Util;
using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Agenix.Api.Validation.Matcher;
using Agenix.Core.Message;
using Agenix.Core.Validation.Xml;
using Microsoft.Extensions.Logging;

namespace Agenix.Validation.Xml.Validation.Matcher.Core;

/// <summary>
///     Validation matcher receives an XML data and validates it against the expected XML with full
///     XML validation support (e.g., ignoring elements, namespace support, ...).
/// </summary>
public class XmlValidationMatcher : IValidationMatcher
{
    /// <summary>CDATA section starting and ending in XML</summary>
    private const string CdataSectionStart = "<![CDATA[";

    private const string CdataSectionEnd = "]]>";

    public const string DefaultXmlMessageValidator = "defaultXmlMessageValidator";

    /// <summary>Logger</summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(DefaultTypeConverter));

    /// <summary>Xml message validator</summary>
    private IMessageValidator<IValidationContext>? _messageValidator;

    public void Validate(string fieldName, string value, List<string> controlParameters, TestContext context)
    {
        var control = controlParameters[0];
        var validationContext = new XmlMessageValidationContext();

        GetMessageValidator(context).ValidateMessage(
            new DefaultMessage(RemoveCDataElements(value)),
            new DefaultMessage(control),
            context,
            [validationContext]);
    }

    /// <summary>
    ///     Find proper XML message validator. Uses several strategies to lookup default XML message validator.
    ///     Caches found validator for future usage once the lookup is done.
    /// </summary>
    /// <param name="context">Test context</param>
    /// <returns>Message validator instance</returns>
    private IMessageValidator<IValidationContext> GetMessageValidator(TestContext context)
    {
        if (_messageValidator != null)
        {
            return _messageValidator;
        }

        // try to find XML message validator in registry
        var defaultMessageValidator = context.MessageValidatorRegistry
            .FindMessageValidator(DefaultXmlMessageValidator);

        if (!defaultMessageValidator.IsPresent)
        {
            try
            {
                defaultMessageValidator = Optional<IMessageValidator<IValidationContext>>.OfNullable(context
                    .ReferenceResolver
                    .Resolve<IMessageValidator<IValidationContext>>(DefaultXmlMessageValidator));
            }
            catch (AgenixSystemException e)
            {
                Log.LogWarning("Unable to find default XML message validator in message validator registry");
            }
        }

        if (!defaultMessageValidator.IsPresent)
        {
            defaultMessageValidator = IMessageValidator<IValidationContext>.Lookup("xml");
        }

        if (defaultMessageValidator.IsPresent)
        {
            _messageValidator = defaultMessageValidator.Value;
            return _messageValidator;
        }

        throw new AgenixSystemException(
            "Unable to locate proper XML message validator - please add validator to project");
    }

    /// <summary>
    ///     Cut off CDATA elements.
    /// </summary>
    /// <param name="value">Input XML string</param>
    /// <returns>XML string without CDATA wrapper</returns>
    private static string RemoveCDataElements(string value)
    {
        var data = value.Trim();

        if (!data.StartsWith(CdataSectionStart))
        {
            return data;
        }

        // Check if it also ends with the CDATA end tag
        if (!data.EndsWith(CdataSectionEnd))
        {
            return data; // Return original if it doesn't end properly
        }

        data = data[CdataSectionStart.Length..];
        data = data[..^CdataSectionEnd.Length];

        return data;
    }
}
