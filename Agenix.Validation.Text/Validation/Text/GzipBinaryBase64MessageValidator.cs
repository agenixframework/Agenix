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

using System.IO.Compression;
using Agenix.Api.Context;
using Agenix.Api.Message;
using Agenix.Api.Validation.Context;

namespace Agenix.Validation.Text.Validation.Text;

/// <summary>
///     Validates messages that are encoded in Base64, compressed with GZIP,
///     and represented as a byte array payload.
/// </summary>
/// <remarks>
///     This validator is a specialization of <see cref="BinaryBase64MessageValidator" />
///     that focuses on handling GZIP compression for the message payloads.
/// </remarks>
public class GzipBinaryBase64MessageValidator : BinaryBase64MessageValidator
{
    /// Validates the received message by decompressing its payload if it's
    /// a byte array and then delegates validation to the base method.
    /// <param name="receivedMessage">The received message to validate.</param>
    /// <param name="controlMessage">The control message used for validation comparison.</param>
    /// <param name="context">The test context containing metadata for the validation process.</param>
    /// <param name="validationContext">The validation context for additional validation configurations.</param>
    public override void ValidateMessage(IMessage receivedMessage, IMessage controlMessage,
        TestContext context, IValidationContext validationContext)
    {
        if (receivedMessage.Payload is byte[] bytes)
        {
            try
            {
                using var gzipInputStream = new GZipStream(new MemoryStream(bytes),
                    CompressionMode.Decompress);
                using var unzipped = new MemoryStream();
                gzipInputStream.CopyTo(unzipped);
                receivedMessage.Payload = unzipped.ToArray();
            }
            catch (IOException e)
            {
                throw new Exception("Failed to validate gzipped message", e);
            }
        }

        base.ValidateMessage(receivedMessage, controlMessage, context, validationContext);
    }

    /// Determines whether the provided message type is supported.
    /// <param name="messageType">The type of the message to validate.</param>
    /// <param name="message">The message instance to validate.</param>
    /// <return>Returns true if the message type is GZIP_BASE64; otherwise, returns false.</return>
    public override bool SupportsMessageType(string messageType, IMessage message)
    {
        return messageType.Equals(nameof(MessageType.GZIP_BASE64), StringComparison.OrdinalIgnoreCase);
    }
}
