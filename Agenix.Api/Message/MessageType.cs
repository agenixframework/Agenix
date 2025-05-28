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
///     Enumeration for message protocol types used in test cases.
/// </summary>
public enum MessageType
{
    UNSPECIFIED,
    XML,
    XHTML,
    CSV,
    JSON,
    PLAINTEXT,
    BINARY,
    BINARY_BASE64,
    GZIP,
    GZIP_BASE64,
    MSCONS
}

/// <summary>
///     Provides extension methods for the MessageType enumeration.
/// </summary>
public class MessageTypeExtensions
{
    public static readonly string FormUrlEncoded = "x-www-form-urlencoded";

    /// Determines whether the specified message type name matches any value in the MessageType enumeration.
    /// @param messageTypeName The name of the message type to be checked.
    /// @return True if the message type name matches a value in the enumeration; otherwise, false.
    /// /
    public static bool Knows(string messageTypeName)
    {
        return Enum.GetValues(typeof(MessageType)).Cast<object>().Any(value =>
            value.ToString()!.Equals(messageTypeName, StringComparison.OrdinalIgnoreCase));
    }

    /// Checks if the given message type is handled as binary content.
    /// @param messageTypeName The name of the message type to evaluate.
    /// @return True if the message type is binary content, otherwise false.
    /// /
    public static bool IsBinary(string messageTypeName)
    {
        return nameof(MessageType.GZIP).Equals(messageTypeName, StringComparison.OrdinalIgnoreCase)
               || nameof(MessageType.BINARY).Equals(messageTypeName, StringComparison.OrdinalIgnoreCase);
    }

    /// Determines whether the specified message type corresponds to XML formats, including XML and XHTML.
    /// <param name="messageType">The message type to be evaluated.</param>
    /// <return>True if the message type is XML or XHTML; otherwise, false.</return>
    public static bool IsXml(string messageType)
    {
        return nameof(MessageType.XML).Equals(messageType, StringComparison.OrdinalIgnoreCase)
               || nameof(MessageType.XHTML).Equals(messageType, StringComparison.OrdinalIgnoreCase);
    }
}
