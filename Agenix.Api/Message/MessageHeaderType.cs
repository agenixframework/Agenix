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

using Agenix.Api.Exceptions;

namespace Agenix.Api.Message;

/// <summary>
///     Represents the various types of message headers.
/// </summary>
public enum MessageHeaderType
{
    INT,
    LONG,
    FLOAT,
    DOUBLE,
    BYTE,
    SHORT,
    BOOL,
    STRING
}

/// <summary>
///     Provides extension methods for handling message header types.
/// </summary>
public static class MessageHeaderTypeExtensions
{
    private static readonly Dictionary<MessageHeaderType, (string Name, Type Clazz)> TypeDetails =
        new()
        {
            { MessageHeaderType.INT, ("int", typeof(int)) },
            { MessageHeaderType.LONG, ("long", typeof(long)) },
            { MessageHeaderType.FLOAT, ("float", typeof(float)) },
            { MessageHeaderType.DOUBLE, ("double", typeof(double)) },
            { MessageHeaderType.BYTE, ("byte", typeof(byte)) },
            { MessageHeaderType.SHORT, ("short", typeof(short)) },
            { MessageHeaderType.BOOL, ("bool", typeof(bool)) },
            { MessageHeaderType.STRING, ("string", typeof(string)) }
        };

    public static readonly string TYPE_PREFIX = "{";
    public static readonly string TYPE_SUFFIX = "}";

    /// <summary>
    ///     Determines whether a given header value is typed according to the
    ///     defined message header type formats.
    /// </summary>
    /// <param name="headerValue">The header value to check.</param>
    /// <returns>True if the header value is typed, otherwise false.</returns>
    public static bool IsTyped(string headerValue)
    {
        return !string.IsNullOrEmpty(headerValue) &&
               Enum.GetValues(typeof(MessageHeaderType))
                   .Cast<MessageHeaderType>()
                   .Any(messageType => headerValue.StartsWith(TYPE_PREFIX + GetName(messageType) + TYPE_SUFFIX));
    }

    /// <summary>
    ///     Creates a typed value by concatenating the provided type and value
    ///     with the defined prefix and suffix.
    /// </summary>
    /// <param name="type">The type to be included in the typed value.</param>
    /// <param name="value">The value to be concatenated with the type.</param>
    /// <returns>A typed value string combining the type and value.</returns>
    public static string CreateTypedValue(string type, string value)
    {
        return TYPE_PREFIX + type + TYPE_SUFFIX + value;
    }

    public static MessageHeaderType FromTypedValue(string headerValue)
    {
        var typeName = headerValue.Substring(1, headerValue.IndexOf(TYPE_SUFFIX, StringComparison.Ordinal) - 1);

        foreach (MessageHeaderType messageType in Enum.GetValues(typeof(MessageHeaderType)))
        {
            if (GetName(messageType).Equals(typeName))
            {
                return messageType;
            }
        }

        throw new AgenixSystemException("Unknown message header type in header value " + headerValue);
    }

    /// <summary>
    ///     Removes the type definition from a given header value if it is typed.
    /// </summary>
    /// <param name="headerValue">The header value from which to remove the type definition.</param>
    /// <returns>The header value without the type definition, if it was present; otherwise, the original header value.</returns>
    public static string RemoveTypeDefinition(string headerValue)
    {
        return IsTyped(headerValue)
            ? headerValue[(headerValue.IndexOf(TYPE_SUFFIX, StringComparison.Ordinal) + TYPE_SUFFIX.Length + 1)..]
            : headerValue;
    }

    /// <summary>
    ///     Gets the name of the specified message header type.
    /// </summary>
    /// <param name="type">The message header type.</param>
    /// <returns>The name of the specified message header type.</returns>
    public static string GetName(this MessageHeaderType type)
    {
        return TypeDetails[type].Name;
    }

    /// <summary>
    ///     Gets the CLR type corresponding to the specified message header type.
    /// </summary>
    /// <param name="type">The message header type.</param>
    /// <returns>The CLR type that corresponds to the specified message header type.</returns>
    public static Type GetClrType(this MessageHeaderType type)
    {
        return TypeDetails[type].Clazz;
    }
}
