using Agenix.Api.Common;
using Agenix.Api.Context;
using Agenix.Api.Message;
using Agenix.Core.Message;
using Agenix.Core.Util;
using Agenix.Core.Validation.Xml;
using Agenix.Validation.Xml.Validation.Xml;

namespace Agenix.Validation.Xml.Validation.Xhtml;

/// <summary>
///     Provides validation mechanisms for XHTML messages by extending the functionality
///     of the base DomXmlMessageValidator class. This validator is specifically designed
///     to handle messages with XHTML content while ensuring XML schema validation is also applied.
/// </summary>
public class XhtmlMessageValidator : DomXmlMessageValidator, InitializingPhase
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

    public override void ValidateMessage(IMessage receivedMessage, IMessage controlMessage, TestContext context,
        XmlMessageValidationContext validationContext)
    {
        var messagePayload = receivedMessage.GetPayload<string>();
        var convertedMessage =
            new DefaultMessage(XhtmlMessageConverter.Convert(messagePayload), receivedMessage.GetHeaders());
        base.ValidateMessage(convertedMessage, controlMessage, context, validationContext);
    }

    public override bool SupportsMessageType(string messageType, IMessage message)
    {
        return messageType.Equals(nameof(MessageType.XHTML), StringComparison.OrdinalIgnoreCase)
               && MessageUtils.HasXmlPayload(message);
    }
}
