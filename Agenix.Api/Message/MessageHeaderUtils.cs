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
///     Provides utility methods for handling message headers in the Agenix.Core.Message namespace.
/// </summary>
public static class MessageHeaderUtils
{
    public const string SEQUENCE_NUMBER = "sequenceNumber";
    public const string SEQUENCE_SIZE = "sequenceSize";
    public const string PRIORITY = "priority";

    /// <summary>
    ///     Determines if the specified header name is considered an internal header used by Spring Integration.
    /// </summary>
    /// <param name="headerName">The name of the header to check.</param>
    /// <returns>True if the header is an internal Spring Integration header; otherwise, false.</returns>
    public static bool IsSpringInternalHeader(string headerName)
    {
        return headerName.StartsWith("springintegration_")
               || headerName.Equals("id")
               || headerName.Equals("timestamp")
               || headerName.Equals("replyChannel")
               || headerName.Equals("errorChannel")
               || headerName.Equals("contentType")
               || headerName.Equals(PRIORITY)
               || headerName.Equals("correlationId")
               || headerName.Equals("routingSlip")
               || headerName.Equals("duplicateMessage")
               || headerName.Equals(SEQUENCE_NUMBER)
               || headerName.Equals(SEQUENCE_SIZE)
               || headerName.Equals("sequenceDetails")
               || headerName.Equals("expirationDate")
               || headerName.StartsWith("jms_");
    }

    /// <summary>
    ///     Sets a specific header in the message, converting its value to the appropriate type if necessary.
    /// </summary>
    /// <param name="message">The message to set the header in.</param>
    /// <param name="name">The name of the header to set.</param>
    /// <param name="value">
    ///     The value of the header to set. This may be converted to the appropriate type based on the header
    ///     name.
    /// </param>
    public static void SetHeader(IMessage message, string name, string value)
    {
        switch (name)
        {
            case SEQUENCE_NUMBER:
                message.SetHeader(SEQUENCE_NUMBER, Convert.ToInt32(value));
                break;
            case SEQUENCE_SIZE:
                message.SetHeader(SEQUENCE_SIZE, Convert.ToInt32(value));
                break;
            case PRIORITY:
                message.SetHeader(PRIORITY, Convert.ToInt32(value));
                break;
            default:
                message.SetHeader(name, value);
                break;
        }
    }

    /// <summary>
    ///     Checks and converts specific header types in the provided headers dictionary.
    /// </summary>
    /// <param name="headers">A dictionary containing message headers.</param>
    public static void CheckHeaderTypes(Dictionary<string, object> headers)
    {
        if (headers.TryGetValue(SEQUENCE_NUMBER, out var value))
        {
            var number = value.ToString();
            headers[SEQUENCE_NUMBER] = Convert.ToInt32(number);
        }

        if (headers.TryGetValue(SEQUENCE_SIZE, out var value1))
        {
            var size = value1.ToString();
            headers[SEQUENCE_SIZE] = Convert.ToInt32(size);
        }

        if (!headers.TryGetValue(PRIORITY, out var value2))
        {
            return;
        }

        {
            var size = value2.ToString();
            headers[PRIORITY] = Convert.ToInt32(size);
        }
    }
}
