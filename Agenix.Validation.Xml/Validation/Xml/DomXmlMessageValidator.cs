#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion


using System.Xml;
using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Util;
using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Agenix.Api.Validation.Matcher;
using Agenix.Api.Xml.Namespace;
using Agenix.Core.Message;
using Agenix.Core.Util;
using Agenix.Core.Validation;
using Agenix.Core.Validation.Xml;
using Agenix.Validation.Xml.Util;
using Agenix.Validation.Xml.Validation.Xml.Schema;
using Microsoft.Extensions.Logging;

namespace Agenix.Validation.Xml.Validation.Xml;

/// <summary>
///     The <c>DomXmlMessageValidator</c> class provides functionality for validating XML messages
///     in a DOM-based manner. It extends the abstract class <see cref="AbstractMessageValidator{T}" />
///     specifically for the <see cref="XmlMessageValidationContext" /> validation context.
/// </summary>
/// <remarks>
///     <c>DomXmlMessageValidator</c> uses XML schema validation to ensure that messages conform
///     to the expected structure and definitions. The validator supports namespace validation
///     and evaluates both message content and schema correctness.
/// </remarks>
public class DomXmlMessageValidator : AbstractMessageValidator<XmlMessageValidationContext>,
    IMessageValidator<IValidationContext>
{
    /// <summary>
    ///     Logger
    /// </summary>
    private static readonly ILogger Logger = LogManager.GetLogger(typeof(DomXmlMessageValidator));

    private readonly XmlSchemaValidation _schemaValidation;

    private NamespaceContextBuilder? _namespaceContextBuilder;

    public DomXmlMessageValidator() : this(new XmlSchemaValidation())
    {
    }

    public DomXmlMessageValidator(XmlSchemaValidation schemaValidation)
    {
        _schemaValidation = schemaValidation;
    }

    /// <summary>
    ///     Determines if the specified message type is supported by the validator.
    /// </summary>
    /// <param name="messageType">The type of the message as a <see cref="string" />.</param>
    /// <param name="message">The <see cref="IMessage" /> instance representing the message.</param>
    /// <returns>
    ///     A <see cref="bool" /> value indicating whether the message type is supported (true) or not (false).
    /// </returns>
    public override bool SupportsMessageType(string messageType, IMessage message)
    {
        return messageType.Equals(nameof(MessageType.XML), StringComparison.OrdinalIgnoreCase) &&
               MessageUtils.HasXmlPayload(message);
    }

    /// <summary>
    ///     Validates the message by performing XML schema validation, namespace validation,
    ///     content validation, and header data validation.
    /// </summary>
    /// <param name="receivedMessage">The received message to validate</param>
    /// <param name="controlMessage">The control message to compare against</param>
    /// <param name="context">The test context</param>
    /// <param name="validationContext">The XML message validation context</param>
    /// <exception cref="System.ComponentModel.DataAnnotations.ValidationException">Thrown when validation fails</exception>
    public override void ValidateMessage(IMessage receivedMessage, IMessage controlMessage,
        TestContext context, XmlMessageValidationContext validationContext)
    {
        if (Logger.IsEnabled(LogLevel.Debug))
        {
            Logger.LogDebug("Start XML message validation: {MessagePayload}",
                XmlUtils.PrettyPrint(receivedMessage.GetPayload<string>()));
        }

        try
        {
            if (validationContext.IsSchemaValidationEnabled)
            {
                _schemaValidation.Validate(receivedMessage, context, validationContext);
            }

            ValidateNamespaces(validationContext.ControlNamespaces, receivedMessage);
            ValidateMessageContent(receivedMessage, controlMessage, validationContext, context);

            if (controlMessage != null)
            {
                if (controlMessage.GetHeaderData().Count > receivedMessage.GetHeaderData().Count)
                {
                    throw new ValidationException($"Failed to validate header data XML fragments - found " +
                                                  $"{receivedMessage.GetHeaderData().Count} header fragments, expected {controlMessage.GetHeaderData().Count}");
                }

                for (var i = 0; i < controlMessage.GetHeaderData().Count; i++)
                {
                    ValidateXmlHeaderFragment(receivedMessage.GetHeaderData()[i],
                        controlMessage.GetHeaderData()[i], validationContext, context);
                }
            }

            Logger.LogDebug("XML message validation successful: All values OK");
        }
        catch (InvalidCastException ex)
        {
            throw new AgenixSystemException(ex.Message);
        }
        catch (XmlException ex)
        {
            throw new AgenixSystemException(ex.Message);
        }
    }


    /// <summary>
    ///     Validate namespaces in a message. The method compares namespace declarations in the root
    ///     element of the received message to expected namespaces. Prefixes are important too, so
    ///     differing namespace prefixes will fail the validation.
    /// </summary>
    /// <param name="expectedNamespaces">Dictionary of expected namespace prefix-URI pairs</param>
    /// <param name="receivedMessage">The received message to validate</param>
    public void ValidateNamespaces(Dictionary<string, string>? expectedNamespaces, IMessage receivedMessage)
    {
        if (expectedNamespaces == null || expectedNamespaces.Count == 0)
        {
            return;
        }

        if (receivedMessage?.Payload == null || !StringUtils.HasText(receivedMessage.GetPayload<string>()))
        {
            throw new ValidationException("Unable to validate message namespaces - receive message payload was empty");
        }

        Logger.LogDebug("Start XML namespace validation");

        var received = XmlUtils.ParseMessagePayload(receivedMessage.GetPayload<string>());

        var foundNamespaces = NamespaceContextBuilder.LookupNamespaces(receivedMessage.GetPayload<string>());

        if (foundNamespaces.Count != expectedNamespaces.Count)
        {
            throw new ValidationException($"Number of namespace declarations not equal for node " +
                                          $"{XmlUtils.GetNodesPathName(received.FirstChild)} found " +
                                          $"{foundNamespaces.Count} expected {expectedNamespaces.Count}");
        }

        foreach (var entry in expectedNamespaces)
        {
            var namespacePrefix = entry.Key;
            var url = entry.Value;

            if (foundNamespaces.ContainsKey(namespacePrefix))
            {
                if (!foundNamespaces[namespacePrefix].Equals(url))
                {
                    throw new ValidationException($"Namespace '{namespacePrefix}' " +
                                                  $"values not equal: found '{foundNamespaces[namespacePrefix]}' " +
                                                  $"expected '{url}' in reference node " +
                                                  $"{XmlUtils.GetNodesPathName(received.FirstChild)}");
                }

                Logger.LogDebug("Validating namespace {NamespacePrefix} value as expected {Url} - value OK",
                    namespacePrefix, url);
            }
            else
            {
                throw new ValidationException($"Missing namespace {namespacePrefix}({url}) in node " +
                                              $"{XmlUtils.GetNodesPathName(received.FirstChild)}");
            }
        }

        Logger.LogDebug("XML namespace validation successful: All values OK");
    }


    /// <summary>
    ///     Validate message payloads by comparing to a control message.
    /// </summary>
    /// <param name="receivedMessage">The received message</param>
    /// <param name="controlMessage">The control message to compare against</param>
    /// <param name="validationContext">The XML message validation context</param>
    /// <param name="context">The test context</param>
    protected void ValidateMessageContent(IMessage receivedMessage, IMessage controlMessage,
        XmlMessageValidationContext validationContext, TestContext context)
    {
        if (controlMessage?.Payload == null)
        {
            Logger.LogDebug("Skip message payload validation as no control message was defined");
            return;
        }

        if (controlMessage.Payload is not string)
        {
            throw new ArgumentException(
                "DomXmlMessageValidator does only support message payload of type String, " +
                "but was " + controlMessage.Payload.GetType());
        }

        var controlMessagePayload = controlMessage.GetPayload<string>();

        if (receivedMessage.Payload == null || !StringUtils.HasText(receivedMessage.GetPayload<string>()))
        {
            if (StringUtils.HasText(controlMessagePayload))
            {
                throw new ValidationException(
                    "Unable to validate message payload - received message payload was empty, control message payload is not");
            }

            return;
        }

        if (!StringUtils.HasText(controlMessagePayload))
        {
            Logger.LogDebug("Skip message payload validation as no control message payload was defined");
            return;
        }

        Logger.LogDebug("Start XML tree validation ...");

        var received = XmlUtils.ParseMessagePayload(receivedMessage.GetPayload<string>());
        var source = XmlUtils.ParseMessagePayload(controlMessagePayload);

        XmlUtils.StripWhitespaceNodes(received);
        XmlUtils.StripWhitespaceNodes(source);

        ValidateXmlTree(received, source, validationContext, GetNamespaceContextBuilder(context)
            .BuildContext(receivedMessage, validationContext.Namespaces), context);
    }

    /// <summary>
    ///     Validates XML header fragment data.
    /// </summary>
    /// <param name="receivedHeaderData">The received header data as XML string</param>
    /// <param name="controlHeaderData">The control header data as XML string</param>
    /// <param name="validationContext">The XML message validation context</param>
    /// <param name="context">The test context</param>
    private void ValidateXmlHeaderFragment(string receivedHeaderData, string controlHeaderData,
        XmlMessageValidationContext validationContext, TestContext context)
    {
        Logger.LogDebug("Start XML header data validation ...");

        var received = XmlUtils.ParseMessagePayload(receivedHeaderData);
        var source = XmlUtils.ParseMessagePayload(controlHeaderData);

        XmlUtils.StripWhitespaceNodes(received);
        XmlUtils.StripWhitespaceNodes(source);

        Logger.LogDebug("Received header data:\n{ReceivedData}", XmlUtils.Serialize(received));
        Logger.LogDebug("Control header data:\n{ControlData}", XmlUtils.Serialize(source));

        ValidateXmlTree(received, source, validationContext,
            GetNamespaceContextBuilder(context)
                .BuildContext(new DefaultMessage(receivedHeaderData), validationContext.Namespaces), context);
    }

    /// <summary>
    ///     Validates element names match between received and source nodes.
    /// </summary>
    /// <param name="received">The received node</param>
    /// <param name="source">The source node</param>
    private void DoElementNameValidation(XmlNode received, XmlNode source)
    {
        // validate element name
        Logger.LogDebug("Validating element: {ElementName} ({NamespaceUri})", received.LocalName,
            received.NamespaceURI);

        if (!received.LocalName.Equals(source.LocalName))
        {
            throw new ValidationException(ValidationUtils.BuildValueMismatchErrorMessage("Element names not equal",
                source.LocalName, received.LocalName));
        }
    }

    /// <summary>
    ///     Handle element node.
    /// </summary>
    /// <param name="received">The received XML node</param>
    /// <param name="source">The source XML node</param>
    /// <param name="validationContext">The XML message validation context</param>
    /// <param name="namespaceContext">The namespace context</param>
    /// <param name="context">The test context</param>
    private void DoElement(XmlNode received, XmlNode source,
        XmlMessageValidationContext validationContext, IXmlNamespaceResolver namespaceContext, TestContext context)
    {
        DoElementNameValidation(received, source);

        DoElementNamespaceValidation(received, source);

        // check if an element is ignored either by xpath or by ignore placeholder in a source message
        if (XmlValidationUtils.IsElementIgnored(source, received, validationContext.IgnoreExpressions,
                namespaceContext))
        {
            return;
        }

        // work on attributes
        Logger.LogDebug("Validating attributes for element: {ElementName}", received.LocalName);
        var receivedAttr = received.Attributes;
        var sourceAttr = source.Attributes;

        if (CountAttributes(receivedAttr) != CountAttributes(sourceAttr))
        {
            throw new ValidationException(ValidationUtils.BuildValueMismatchErrorMessage(
                "Number of attributes not equal for element '" +
                received.LocalName + "'", CountAttributes(sourceAttr), CountAttributes(receivedAttr)));
        }

        for (var i = 0; i < receivedAttr.Count; i++)
        {
            DoAttribute(received, receivedAttr[i], source, validationContext, namespaceContext, context);
        }

        // check if validation matcher on element is specified
        if (IsValidationMatcherExpression(source))
        {
            ValidationMatcherUtils.ResolveValidationMatcher(source.Name,
                received.FirstChild.Value.Trim(),
                source.FirstChild.Value.Trim(),
                context);
            return;
        }

        DoText((XmlElement)received, (XmlElement)source);

        // work on child nodes
        var receivedChildElements = DomUtils.GetChildElements((XmlElement)received);
        var sourceChildElements = DomUtils.GetChildElements((XmlElement)source);

        if (receivedChildElements.Count != sourceChildElements.Count)
        {
            throw new ValidationException(ValidationUtils.BuildValueMismatchErrorMessage(
                "Number of child elements not equal for element '" +
                received.LocalName + "'", sourceChildElements.Count, receivedChildElements.Count));
        }

        for (var i = 0; i < receivedChildElements.Count; i++)
        {
            ValidateXmlTree(receivedChildElements[i], sourceChildElements[i],
                validationContext, namespaceContext, context);
        }

        Logger.LogDebug("Validation successful for element: {ElementName} ({NamespaceUri})", received.LocalName,
            received.NamespaceURI);
    }


    /// <summary>
    ///     Handle document type definition with validation of publicId and systemId.
    /// </summary>
    /// <param name="received">The received XML node</param>
    /// <param name="source">The source XML node</param>
    /// <param name="validationContext">The XML message validation context</param>
    /// <param name="namespaceContext">The namespace context</param>
    /// <param name="context">The test context</param>
    private void DoDocumentTypeDefinition(XmlNode received, XmlNode source,
        XmlMessageValidationContext validationContext,
        IXmlNamespaceResolver namespaceContext, TestContext context)
    {
        if (source is not XmlDocumentType sourceDTD)
        {
            throw new ValidationException("Missing document type definition in expected xml fragment");
        }

        var receivedDTD = (XmlDocumentType)received;

        Logger.LogDebug("Validating document type definition: {PublicId} ({SystemId})", receivedDTD.PublicId,
            receivedDTD.SystemId);

        if (!StringUtils.HasText(sourceDTD.PublicId))
        {
            if (receivedDTD.PublicId != null)
            {
                throw new ValidationException(ValidationUtils.BuildValueMismatchErrorMessage(
                    "Document type public id not equal",
                    sourceDTD.PublicId, receivedDTD.PublicId));
            }
        }
        else if (sourceDTD.PublicId.Trim().Equals(AgenixSettings.IgnorePlaceholder))
        {
            Logger.LogDebug("Document type public id: '{PublicId}' is ignored by placeholder '{Placeholder}'",
                receivedDTD.PublicId, AgenixSettings.IgnorePlaceholder);
        }
        else if (!StringUtils.HasText(receivedDTD.PublicId) || !receivedDTD.PublicId.Equals(sourceDTD.PublicId))
        {
            throw new ValidationException(ValidationUtils.BuildValueMismatchErrorMessage(
                "Document type public id not equal",
                sourceDTD.PublicId, receivedDTD.PublicId));
        }

        if (!StringUtils.HasText(sourceDTD.SystemId))
        {
            if (receivedDTD.SystemId != null)
            {
                throw new ValidationException(ValidationUtils.BuildValueMismatchErrorMessage(
                    "Document type system id not equal",
                    sourceDTD.SystemId, receivedDTD.SystemId));
            }
        }
        else if (sourceDTD.SystemId.Trim().Equals(AgenixSettings.IgnorePlaceholder))
        {
            Logger.LogDebug("Document type system id: '{SystemId}' is ignored by placeholder '{Placeholder}'",
                receivedDTD.SystemId, AgenixSettings.IgnorePlaceholder);
        }
        else if (!StringUtils.HasText(receivedDTD.SystemId) || !receivedDTD.SystemId.Equals(sourceDTD.SystemId))
        {
            throw new ValidationException(ValidationUtils.BuildValueMismatchErrorMessage(
                "Document type system id not equal",
                sourceDTD.SystemId, receivedDTD.SystemId));
        }

        ValidateXmlTree(received.NextSibling,
            source.NextSibling, validationContext, namespaceContext, context);
    }


    /// <summary>
    ///     Validates XML tree structure recursively based on a node type.
    /// </summary>
    /// <param name="received">The received XML node</param>
    /// <param name="source">The source XML node</param>
    /// <param name="validationContext">The XML message validation context</param>
    /// <param name="namespaceContext">The namespace context</param>
    /// <param name="context">The test context</param>
    private void ValidateXmlTree(XmlNode received, XmlNode source,
        XmlMessageValidationContext validationContext, IXmlNamespaceResolver namespaceContext, TestContext context)
    {
        switch (received.NodeType)
        {
            case XmlNodeType.DocumentType:
                DoDocumentTypeDefinition(received, source, validationContext, namespaceContext, context);
                break;
            case XmlNodeType.Document:
                ValidateXmlTree(received.FirstChild, source.FirstChild,
                    validationContext, namespaceContext, context);
                break;
            case XmlNodeType.Element:
                DoElement(received, source, validationContext, namespaceContext, context);
                break;
            case XmlNodeType.Attribute:
                throw new InvalidOperationException();
            case XmlNodeType.Comment:
                ValidateXmlTree(received.NextSibling, source,
                    validationContext, namespaceContext, context);
                break;
            case XmlNodeType.ProcessingInstruction:
                DoPI(received);
                break;
        }
    }

    /// <summary>
    ///     Validates element namespaces match between received and source nodes.
    /// </summary>
    /// <param name="received">The received node</param>
    /// <param name="source">The source node</param>
    private void DoElementNamespaceValidation(XmlNode received, XmlNode source)
    {
        // validate element namespace
        Logger.LogDebug("Validating namespace for element: {ElementName}", received.LocalName);

        if (received.NamespaceURI != null)
        {
            if (source.NamespaceURI == null)
            {
                throw new ValidationException(ValidationUtils.BuildValueMismatchErrorMessage(
                    "Element namespace not equal for element '" +
                    received.LocalName + "'", null, received.NamespaceURI));
            }

            if (!received.NamespaceURI.Equals(source.NamespaceURI))
            {
                throw new ValidationException(ValidationUtils.BuildValueMismatchErrorMessage(
                    "Element namespace not equal for element '" +
                    received.LocalName + "'", source.NamespaceURI, received.NamespaceURI));
            }
        }
        else if (source.NamespaceURI != null)
        {
            throw new ValidationException(ValidationUtils.BuildValueMismatchErrorMessage(
                "Element namespace not equal for element '" +
                received.LocalName + "'", source.NamespaceURI, null));
        }
    }

    /// <summary>
    ///     Handle text node during validation.
    /// </summary>
    /// <param name="received">The received element</param>
    /// <param name="source">The source element</param>
    private void DoText(XmlElement received, XmlElement source)
    {
        Logger.LogDebug("Validating node value for element: {ElementName}", received.LocalName);

        var receivedText = DomUtils.GetTextValue(received);
        var sourceText = DomUtils.GetTextValue(source);

        if (!receivedText.Trim().Equals(sourceText.Trim()))
        {
            throw new ValidationException(ValidationUtils.BuildValueMismatchErrorMessage(
                "Node value not equal for element '"
                + received.LocalName + "'", sourceText.Trim(), receivedText.Trim()));
        }

        if (Logger.IsEnabled(LogLevel.Debug))
        {
            Logger.LogDebug("Node value '{NodeValue}': OK", receivedText.Trim());
        }
    }

    /// <summary>
    ///     Handle attribute node during validation.
    /// </summary>
    /// <param name="receivedElement">The received element node</param>
    /// <param name="receivedAttribute">The received attribute node</param>
    /// <param name="sourceElement">The source element node</param>
    /// <param name="validationContext">The XML message validation context</param>
    /// <param name="namespaceContext">The namespace context</param>
    /// <param name="context">The test context</param>
    private void DoAttribute(XmlNode receivedElement, XmlNode receivedAttribute, XmlNode sourceElement,
        XmlMessageValidationContext validationContext, IXmlNamespaceResolver namespaceContext, TestContext context)
    {
        if (receivedAttribute.Name.StartsWith("xmlns")) { return; }

        var receivedAttributeName = receivedAttribute.LocalName;

        Logger.LogDebug("Validating attribute: {AttributeName} ({NamespaceUri})", receivedAttributeName,
            receivedAttribute.NamespaceURI);

        var sourceAttributes = sourceElement.Attributes;
        var sourceAttribute = sourceAttributes.GetNamedItem(receivedAttributeName, receivedAttribute.NamespaceURI);

        if (sourceAttribute == null)
        {
            throw new ValidationException("Attribute validation failed for element '"
                                          + receivedElement.LocalName + "', unknown attribute "
                                          + receivedAttributeName + " (" + receivedAttribute.NamespaceURI + ")");
        }

        if (XmlValidationUtils.IsAttributeIgnored(receivedElement, receivedAttribute, sourceAttribute,
                validationContext.IgnoreExpressions, namespaceContext))
        {
            return;
        }

        var receivedValue = receivedAttribute.Value;
        var sourceValue = sourceAttribute.Value;
        if (IsValidationMatcherExpression(sourceAttribute))
        {
            ValidationMatcherUtils.ResolveValidationMatcher(sourceAttribute.Name,
                receivedAttribute.Value.Trim(),
                sourceAttribute.Value.Trim(),
                context);
        }
        else if (receivedValue.Contains(':') && sourceValue.Contains(':'))
        {
            DoNamespaceQualifiedAttributeValidation(receivedElement, receivedAttribute, sourceElement, sourceAttribute);
        }
        else
        {
            if (!receivedValue.Equals(sourceValue))
            {
                throw new ValidationException(ValidationUtils.BuildValueMismatchErrorMessage(
                    "Values not equal for attribute '"
                    + receivedAttributeName + "'", sourceValue, receivedValue));
            }
        }

        Logger.LogDebug("Attribute '{AttributeName}'='{AttributeValue}': OK", receivedAttributeName, receivedValue);
    }

    /// <summary>
    ///     Perform validation on namespace qualified attribute values if present. This includes the validation of namespace
    ///     presence
    ///     and equality.
    /// </summary>
    /// <param name="receivedElement">The received element node</param>
    /// <param name="receivedAttribute">The received attribute node</param>
    /// <param name="sourceElement">The source element node</param>
    /// <param name="sourceAttribute">The source attribute node</param>
    private void DoNamespaceQualifiedAttributeValidation(XmlNode receivedElement, XmlNode receivedAttribute,
        XmlNode sourceElement, XmlNode sourceAttribute)
    {
        var receivedValue = receivedAttribute.Value;
        var sourceValue = sourceAttribute.Value;

        if (receivedValue.Contains(':') && sourceValue.Contains(':'))
        {
            // value has a namespace prefix set, do special QName validation
            var receivedPrefix = receivedValue[..receivedValue.IndexOf(':')];
            var sourcePrefix = sourceValue[..sourceValue.IndexOf(':')];

            var receivedNamespaces = XmlUtils.LookupNamespaces(receivedAttribute.OwnerDocument);
            foreach (var ns in XmlUtils.LookupNamespaces(receivedElement))
            {
                receivedNamespaces[ns.Key] = ns.Value;
            }

            if (receivedNamespaces.ContainsKey(receivedPrefix))
            {
                var sourceNamespaces = XmlUtils.LookupNamespaces(sourceAttribute.OwnerDocument);
                foreach (var ns in XmlUtils.LookupNamespaces(sourceElement))
                {
                    sourceNamespaces[ns.Key] = ns.Value;
                }

                if (sourceNamespaces.ContainsKey(sourcePrefix))
                {
                    if (!sourceNamespaces[sourcePrefix].Equals(receivedNamespaces[receivedPrefix]))
                    {
                        throw new ValidationException(ValidationUtils.BuildValueMismatchErrorMessage(
                            "Values not equal for attribute value namespace '"
                            + receivedValue + "'", sourceNamespaces[sourcePrefix], receivedNamespaces[receivedPrefix]));
                    }

                    // remove namespace prefixes as they must not form equality
                    receivedValue = receivedValue[(receivedPrefix + ":").Length..];
                    sourceValue = sourceValue[(sourcePrefix + ":").Length..];
                }
                else
                {
                    throw new ValidationException("Received attribute value '" + receivedAttribute.LocalName +
                                                  "' describes namespace qualified attribute value," +
                                                  " control value '" + sourceValue + "' does not");
                }
            }
        }

        if (!receivedValue.Equals(sourceValue))
        {
            throw new ValidationException(ValidationUtils.BuildValueMismatchErrorMessage(
                "Values not equal for attribute '"
                + receivedAttribute.LocalName + "'", sourceValue, receivedValue));
        }
    }

    /// <summary>
    ///     Handle processing instruction during validation.
    /// </summary>
    /// <param name="received">The processing instruction node</param>
    private void DoPI(XmlNode received)
    {
        Logger.LogDebug("Ignored processing instruction ({Name}={Value})", received.LocalName, received.Value);
    }

    /// <summary>
    ///     Counts the attribute nodes for an element (xmlns attributes ignored)
    /// </summary>
    /// <param name="attributesR">attributes map</param>
    /// <returns>number of attributes</returns>
    private int CountAttributes(XmlAttributeCollection attributesR)
    {
        var cntAttributes = 0;

        for (var i = 0; i < attributesR.Count; i++)
        {
            if (!attributesR[i].Name.StartsWith("xmlns"))
            {
                cntAttributes++;
            }
        }

        return cntAttributes;
    }

    /// <summary>
    ///     Checks whether the given node contains a validation matcher
    /// </summary>
    /// <param name="node">The node to check</param>
    /// <returns>true if node value contains validation matcher, false if not</returns>
    private bool IsValidationMatcherExpression(XmlNode node)
    {
        return node.NodeType switch
        {
            XmlNodeType.Element =>
                node.FirstChild != null
                && StringUtils.HasText(node.FirstChild.Value)
                && ValidationMatcherUtils.IsValidationMatcherExpression(node.FirstChild.Value.Trim()),
            XmlNodeType.Attribute =>
                StringUtils.HasText(node.Value)
                && ValidationMatcherUtils.IsValidationMatcherExpression(node.Value.Trim()),
            _ => false // validation matchers make no sense
        };
    }


    /// <summary>
    ///     Finds and returns a specific validation context of type <see cref="XmlMessageValidationContext" />
    ///     from a provided list of validation contexts. If the context is not found, it adapts a
    ///     <see cref="DefaultMessageValidationContext" /> for usage if applicable.
    /// </summary>
    /// <param name="validationContexts">
    ///     A list of validation contexts to search through, represented as
    ///     <see cref="List{IValidationContext}" />.
    /// </param>
    /// <returns>
    ///     An instance of <see cref="XmlMessageValidationContext" /> if found or created; otherwise, uses the base
    ///     implementation to return a context.
    /// </returns>
    public override XmlMessageValidationContext FindValidationContext(List<IValidationContext> validationContexts)
    {
        if (!validationContexts.Any(context => context is XmlMessageValidationContext))
        {
            var messageValidationContext = validationContexts
                .Where(context => context.GetType() == typeof(DefaultMessageValidationContext))
                .Cast<DefaultMessageValidationContext>()
                .FirstOrDefault();

            if (messageValidationContext != null)
            {
                return XmlMessageValidationContext.Builder.Adapt(messageValidationContext).Build();
            }
        }

        return base.FindValidationContext(validationContexts);
    }

    /// <summary>
    ///     Retrieves the required type of validation context for the validator.
    /// </summary>
    /// <returns>
    ///     The <see cref="System.Type" /> instance representing the required validation context type,
    ///     which is <see cref="XmlMessageValidationContext" /> for this validator.
    /// </returns>
    protected override Type GetRequiredValidationContextType()
    {
        return typeof(XmlMessageValidationContext);
    }

    /// <summary>
    ///     Get an explicit namespace context builder set on this class or get instance from reference resolver.
    /// </summary>
    private NamespaceContextBuilder GetNamespaceContextBuilder(TestContext context)
    {
        return _namespaceContextBuilder ?? XmlValidationHelper.GetNamespaceContextBuilder(context);
    }

    /// <summary>
    ///     Sets the namespace context builder.
    /// </summary>
    public void SetNamespaceContextBuilder(NamespaceContextBuilder namespaceContextBuilder)
    {
        _namespaceContextBuilder = namespaceContextBuilder;
    }

    /// <summary>
    ///     Validates the XML schema of the provided message using the given context and validation context.
    /// </summary>
    /// <param name="message">The <see cref="IMessage" /> instance representing the message to be validated.</param>
    /// <param name="context">The <see cref="TestContext" /> containing information and state related to the test execution.</param>
    /// <param name="xmlMessageValidationContext">
    ///     The <see cref="XmlMessageValidationContext" /> providing validation-specific
    ///     data for XML schema validation.
    /// </param>
    public void ValidateXmlSchema(IMessage message, TestContext context,
        XmlMessageValidationContext xmlMessageValidationContext)
    {
        _schemaValidation.Validate(message, context, xmlMessageValidationContext);
    }
}
