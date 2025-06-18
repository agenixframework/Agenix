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

using Agenix.Api.Message;

namespace Agenix.Http.Message;

/// Utility class for handling operations related to HTTP messages.
/// Provides methods to copy settings from one message to another,
/// ensuring compatibility and preserving headers and properties.
/// /
public static class HttpMessageUtils
{
    /// Copies settings from a source message to a target HTTP message, converting if necessary.
    /// @param from The source message, which can be either an IMessage or HttpMessage.
    /// @param to The target HTTP message to which settings will be applied.
    /// /
    public static void Copy(IMessage from, HttpMessage to)
    {
        HttpMessage source;
        if (from is HttpMessage httpMessage)
        {
            source = httpMessage;
        }
        else
        {
            source = new HttpMessage(from);
        }

        Copy(source, to);
    }

    /// Copies the properties and headers from one HttpMessage instance to another.
    /// @param from the source HttpMessage from which properties are to be copied
    /// @param to the target HttpMessage to which properties are to be copied
    /// /
    public static void Copy(HttpMessage from, HttpMessage to)
    {
        to.Name = from.Name;
        to.SetType(from.GetType());
        to.Payload = from.Payload;

        foreach (var entry in from.GetHeaders().Where(entry =>
                     !entry.Key.Equals(MessageHeaders.Id) && !entry.Key.Equals(MessageHeaders.Timestamp)))
        {
            to.Header(entry.Key, entry.Value);
        }

        foreach (var headerData in from.GetHeaderData())
        {
            to.AddHeaderData(headerData);
        }

        foreach (var cookie in from.GetCookies())
        {
            to.Cookie(cookie);
        }
    }
}
