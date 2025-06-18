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
using Agenix.Api.Log;
using Microsoft.Extensions.Logging;

namespace Agenix.Api.Message;

/// <summary>
///     The AbstractMessageProcessor is an abstract class that provides a base implementation
///     for processing messages in defined directions with specific types.
///     It implements the following interfaces:
///     - IMessageProcessor: Extends the capability of message transformations.
///     - IMessageDirectionAware: Enables awareness and management of message directionality
///     (inbound, outbound, unbound).
///     - IMessageTypeSelector: Supports type-based selection and filtering of messages.
///     The class provides mechanisms for message direction handling, checking message
///     compatibility based on type, and processing messages with contextual information.
///     Subclasses can extend the functionality by providing their specific implementations
///     for processing and type selection.
/// </summary>
public abstract class AbstractMessageProcessor : IMessageProcessor, IMessageDirectionAware, IMessageTypeSelector
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(AbstractMessageProcessor));

    /// <summary>
    ///     Inbound/ Outbound direction
    /// </summary>
    private MessageDirection _direction = MessageDirection.UNBOUND;

    /// <summary>
    ///     Inbound/ Outbound direction
    /// </summary>
    public MessageDirection Direction
    {
        set => _direction = value;
    }

    /// <summary>
    ///     Retrieves the current direction of message processing for this message processor.
    /// </summary>
    /// <returns>The current message direction (inbound, outbound, or unbound).</returns>
    public MessageDirection GetDirection()
    {
        return _direction;
    }

    /// <summary>
    ///     Processes the given message if it is supported by this processor.
    /// </summary>
    /// <param name="message">The message to be processed.</param>
    /// <param name="context">The current test context.</param>
    public void Process(IMessage message, TestContext context)
    {
        if (SupportsMessageType(message.GetType()))
        {
            ProcessMessage(message, context);
        }
        else
        {
            Log.LogDebug("Message processor '{S}' skipped for message type: {GetType1}", GetName(), message.GetType());
        }
    }

    /// <summary>
    ///     Checks if this message processor is capable of handling the given message type.
    /// </summary>
    /// <param name="messageType">The message type representation as string (e.g., xml, json, csv, plaintext).</param>
    /// <returns>true if this component supports the message type.</returns>
    public virtual bool SupportsMessageType(string messageType)
    {
        return true;
    }

    /// <summary>
    ///     Subclasses may overwrite this method in order to modify payload and/ or headers of the processed message.
    /// </summary>
    /// <param name="message">the message to process.</param>
    /// <param name="context">the current test context.</param>
    public virtual void ProcessMessage(IMessage message, TestContext context)
    {
        // subclasses may implement processing logic
    }

    /// <summary>
    ///     Retrieves the name of the message processor class.
    /// </summary>
    /// <returns>The name of the message processor class.</returns>
    protected virtual string GetName()
    {
        return GetType().Name;
    }
}
