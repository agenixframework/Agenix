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


using Agenix.Api.Context;
using Agenix.Api.Message;
using Agenix.Validation.Xml.Util;

namespace Agenix.Validation.Xml;

/// <summary>
/// The XmlFormattingMessageProcessor is responsible for processing messages
/// where the payload is expected to be in XML format. The processor formats
/// the XML payload into a pretty-printed format for enhanced readability.
/// </summary>
/// <remarks>
/// This class extends the AbstractMessageProcessor and overrides specific
/// methods to provide custom processing for XML messages. It performs message
/// type selection and content transformation specific to XML payloads.
/// </remarks>
/// <example>
/// Intended to process only messages of the type "XML". It ensures the
/// payload is transformed into a clean and formatted XML structure.
/// </example>
public class XmlFormattingMessageProcessor :  AbstractMessageProcessor
{
    /// <summary>
    /// Processes the provided message by applying XML formatting to its payload.
    /// </summary>
    /// <param name="message">The message to process. Its payload is expected to be XML content.</param>
    /// <param name="context">The context in which the message is being processed. This provides additional metadata or dependencies needed for processing.</param>
    public override void ProcessMessage(IMessage message, TestContext context)
    {
        message.Payload = (XmlUtils.PrettyPrint(message.GetPayload<string>()));
    }

    /// <summary>
    /// Determines whether the given message type is supported by this processor.
    /// </summary>
    /// <param name="messageType">The type of the message to evaluate for support. Possible values are defined in the <see cref="MessageType"/> enumeration.</param>
    /// <returns>True if the message type is supported; otherwise, false.</returns>
    public override bool SupportsMessageType(string messageType)
    {
        return string.Equals(messageType, nameof(MessageType.XML), StringComparison.OrdinalIgnoreCase);
    }

}
