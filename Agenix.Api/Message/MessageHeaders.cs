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

namespace Agenix.Api.Message;

/// <summary>
///     Specific message headers.
/// </summary>
public class MessageHeaders
{
    /// <summary>
    ///     Common header name prefix
    /// </summary>
    public static readonly string Prefix = "agenix_";

    /// <summary>
    ///     Message-related header prefix
    /// </summary>
    public static readonly string MessagePrefix = Prefix + "message_";

    /// <summary>
    ///     Unique message id
    /// </summary>
    public static readonly string Id = MessagePrefix + "id";

    /// <summary>
    ///     Time message was created
    /// </summary>
    public static readonly string Timestamp = MessagePrefix + "timestamp";

    /// <summary>
    ///     Header indicating the message type (e.g., XML, JSON, csv, plaintext, etc.)
    /// </summary>
    public static readonly string MessageType = MessagePrefix + "type";

    /// <summary>
    ///     Synchronous message correlation
    /// </summary>
    public static readonly string MessageCorrelationKey = MessagePrefix + "correlator";

    /// <summary>
    ///     Synchronous reply to message destination name
    /// </summary>
    public static readonly string MessageReplyTo = MessagePrefix + "replyTo";

    /// <summary>
    ///     Prevent instantiation.
    /// </summary>
    private MessageHeaders()
    {
    }
}
