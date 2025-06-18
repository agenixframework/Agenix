using Agenix.Api.Common;
using Agenix.Api.Context;
using Agenix.Api.Message;
using Agenix.Core.Message;
using Agenix.Core.Util;
using Agenix.Core.Validation.Xml;
using Agenix.Validation.Xml.Validation.Xml;

namespace Agenix.Validation.Xml.Validation.Xhtml;

/// <summary>
///     Represents a validator for validating XHTML messages using XPath expressions.
///     This validator validates messages after converting their payload into XHTML-compatible format.
/// </summary>
public class XhtmlXpathMessageValidator : XpathMessageValidator, InitializingPhase
{
    /// <summary>
    ///     Represents the property that allows getting or setting the instance of
    ///     <see cref="XhtmlMessageConverter" /> used for converting HTML content to
    ///     XHTML format and other related operations.
    /// </summary>
    public XhtmlMessageConverter XhtmlMessageConverter { get; set; } = new();

    public void Initialize()
    {
        XhtmlMessageConverter.Initialize();
    }

    public override void ValidateMessage(
        IMessage receivedMessage,
        IMessage controlMessage,
        TestContext context,
        XpathMessageValidationContext validationContext)
    {
        var messagePayload = receivedMessage.GetPayload<string>();
        var convertedMessage = new DefaultMessage(
            XhtmlMessageConverter.Convert(messagePayload),
            receivedMessage.GetHeaders()
        );
        base.ValidateMessage(convertedMessage, controlMessage, context, validationContext);
    }

    public override bool SupportsMessageType(string messageType, IMessage message)
    {
        return messageType.Equals(nameof(MessageType.XHTML), StringComparison.OrdinalIgnoreCase)
               && MessageUtils.HasXmlPayload(message);
    }
}
