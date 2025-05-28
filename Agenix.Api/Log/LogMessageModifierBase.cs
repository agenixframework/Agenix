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

namespace Agenix.Api.Log;

/// <summary>
///     Special modifier adds message-related modifications on logger output on headers and body.
/// </summary>
public abstract class LogMessageModifierBase : ILogModifier
{
    public abstract string Mask(string statement);

    /// <summary>
    ///     Mask the given message body to not print sensitive data.
    /// </summary>
    /// <param name="message">the message</param>
    /// <returns></returns>
    public string MaskBody(IMessage message)
    {
        return Mask(message.GetPayload<string>().Trim());
    }

    /// <summary>
    ///     Mask the given message header values to not print sensitive data.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public Dictionary<string, object> MaskHeaders(IMessage message)
    {
        var newDict = new Dictionary<string, object>();
        foreach (var entry in message.GetHeaders())
        {
            if (entry.Value == null)
            {
                newDict.Add(entry.Key, "");
                continue;
            }

            var keyValuePair = $"{entry.Key}={entry.Value}";
            if (!keyValuePair.Equals(Mask(keyValuePair)))
            {
                newDict.Add(entry.Key, AgenixSettings.GetLogMaskValue());
                continue;
            }

            newDict.Add(entry.Key, entry.Value);
        }

        return newDict;
    }


    public List<string> MaskHeaderData(IMessage message)
    {
        if (message.GetHeaderData == null || message.GetHeaderData().Count == 0) return [];

        return message.GetHeaderData().Select(Mask).ToList();
    }
}
