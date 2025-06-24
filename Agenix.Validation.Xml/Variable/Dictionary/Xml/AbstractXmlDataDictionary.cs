#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
// 
// Copyright (c) 2025 Agenix
// 
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

#endregion

using System.Xml;
using Agenix.Api.Context;
using Agenix.Api.Message;
using Agenix.Core.Variable.Dictionary;
using Agenix.Validation.Xml.Util;
using Agenix.Validation.Xml.Validation.Xhtml;

namespace Agenix.Validation.Xml.Variable.Dictionary.Xml;

/// <summary>
///     Abstract data dictionary works on XML message payloads only with parsing the document and translating each element
///     and attribute with respective value in dictionary.
/// </summary>
public abstract class AbstractXmlDataDictionary : AbstractDataDictionary<XmlNode>
{
    public override void ProcessMessage(IMessage message, TestContext context)
    {
        if (message.Payload == null || string.IsNullOrWhiteSpace(message.GetPayload<string>()))
        {
            return;
        }

        var messagePayload = message.GetPayload<string>();
        if (string.Equals(nameof(MessageType.XHTML), message.GetType(), StringComparison.OrdinalIgnoreCase))
        {
            messagePayload = new XhtmlMessageConverter().Convert(messagePayload);
        }

        var doc = XmlUtils.ParseMessagePayload(messagePayload);
        var xmlConfigurer = new XmlConfigurer();
        var targetEncoding = XmlUtils.GetTargetEncoding(doc);


        using var memoryStream = new MemoryStream();
        xmlConfigurer.AddSerializeSetting(XmlConfigurer.Encoding, targetEncoding);

        using var xmlWriter = xmlConfigurer.CreateXmlWriter(memoryStream);

        WriteNodeWithTranslation(doc, xmlWriter, context);
        xmlWriter.Flush();

        // Use the same encoding to read back the string
        message.Payload = targetEncoding.GetString(memoryStream.ToArray()).TrimStart('\uFEFF');
    }


    /// <summary>
    ///     Recursively writes XML nodes while applying translations to elements and attributes.
    /// </summary>
    /// <param name="node">The XML node to process</param>
    /// <param name="writer">The XML writer</param>
    /// <param name="context">The test context</param>
    private void WriteNodeWithTranslation(XmlNode node, XmlWriter writer, TestContext context)
    {
        switch (node.NodeType)
        {
            case XmlNodeType.Document:
                foreach (XmlNode childNode in node.ChildNodes)
                {
                    WriteNodeWithTranslation(childNode, writer, context);
                }

                break;

            case XmlNodeType.Element:
                var element = (XmlElement)node;

                // Use the 3-parameter overload to preserve the original prefix
                writer.WriteStartElement(element.Prefix, element.LocalName, element.NamespaceURI);

                // Process attributes
                if (element.HasAttributes)
                {
                    foreach (XmlAttribute attribute in element.Attributes)
                    {
                        var translatedValue = Translate(attribute, attribute.Value, context);

                        // Preserve attribute prefixes
                        if (string.IsNullOrEmpty(attribute.NamespaceURI))
                        {
                            writer.WriteAttributeString(attribute.LocalName, translatedValue);
                        }
                        else
                        {
                            writer.WriteAttributeString(attribute.Prefix, attribute.LocalName, attribute.NamespaceURI,
                                translatedValue);
                        }
                    }
                }

                // Process child nodes
                if (element.HasChildNodes)
                {
                    // Check if element has only text content
                    if (element.ChildNodes.Count == 1 && element.FirstChild.NodeType == XmlNodeType.Text)
                    {
                        var textContent = element.InnerText;
                        var translatedText = Translate(element, textContent, context);
                        writer.WriteString(translatedText);
                    }
                    else
                    {
                        foreach (XmlNode childNode in element.ChildNodes)
                        {
                            WriteNodeWithTranslation(childNode, writer, context);
                        }
                    }
                }
                else
                {
                    // Empty element - check if translation provides content
                    var translated = Translate(element, string.Empty, context);
                    if (!string.IsNullOrWhiteSpace(translated))
                    {
                        writer.WriteString(translated);
                    }
                }

                writer.WriteEndElement();
                break;

            case XmlNodeType.Text:
                writer.WriteString(node.Value);
                break;

            case XmlNodeType.CDATA:
                writer.WriteCData(node.Value);
                break;

            case XmlNodeType.Comment:
                writer.WriteComment(node.Value);
                break;

            case XmlNodeType.ProcessingInstruction:
                var pi = (XmlProcessingInstruction)node;
                writer.WriteProcessingInstruction(pi.Target, pi.Data);
                break;

            case XmlNodeType.XmlDeclaration:
                // XmlWriter handles declaration automatically based on settings
                break;
        }
    }

    public override bool SupportsMessageType(string messageType)
    {
        return string.Equals(nameof(MessageType.XML), messageType, StringComparison.OrdinalIgnoreCase) ||
               string.Equals(nameof(MessageType.XHTML), messageType, StringComparison.OrdinalIgnoreCase);
    }
}
