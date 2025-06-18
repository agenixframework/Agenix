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

using System.Text;

namespace Agenix.Api.Message;

public static class MessagePayloadUtils
{
    private static readonly bool prettyPrint = AgenixSettings.IsPrettyPrintEnabled();

    /// <summary>
    ///     Pretty print given message payload. Supports XML/ JSON payloads.
    /// </summary>
    /// <param name="payload"></param>
    /// <returns></returns>
    public static string PrettyPrint(string payload)
    {
        if (!prettyPrint)
        {
            return payload;
        }

        if (IsXml(payload))
        {
            return PrettyPrintXml(payload);
        }

        if (IsJson(payload))
        {
            return PrettyPrintJson(payload);
        }

        return payload;
    }

    /// <summary>
    ///     Checks if the given message payload is of XML nature.
    /// </summary>
    /// <param name="payload"></param>
    /// <returns></returns>
    public static bool IsXml(string payload)
    {
        return payload.Trim().StartsWith('<');
    }

    /// <summary>
    ///     Check if the given message payload is of Json nature.
    /// </summary>
    /// <param name="payload"></param>
    /// <returns></returns>
    public static bool IsJson(string payload)
    {
        return payload.Trim().StartsWith('{') || payload.Trim().StartsWith('[');
    }

    /// <summary>
    ///     Pretty print given XML payload.
    /// </summary>
    /// <param name="payload"></param>
    /// <returns></returns>
    public static string PrettyPrintXml(string payload)
    {
        var singleLine = true;
        var indentNum = 2;
        var indent = 0;

        var s = payload.Trim();
        StringBuilder sb = new();
        for (var i = 0; i < s.Length; i++)
        {
            var currentChar = s[i];

            if (currentChar == '<')
            {
                var nextChar = s[i + 1];
                if (nextChar == '/')
                {
                    indent -= indentNum;
                }

                if (!singleLine)
                {
                    sb.Append(' ', indent);
                }

                if (nextChar != '?' && nextChar != '!' && nextChar != '/')
                {
                    indent += indentNum;
                }

                singleLine = false;
            }

            sb.Append(currentChar);
            if (currentChar == '>')
            {
                if (s[i - 1] == '/')
                {
                    indent -= indentNum;
                    sb.Append(Environment.NewLine);
                }
                else
                {
                    var nextStartElementPos = s.IndexOf('<', i);
                    if (nextStartElementPos > i + 1)
                    {
                        var textBetweenElements = s[(i + 1)..nextStartElementPos];

                        if (textBetweenElements.Trim().Length == 0)
                        {
                            sb.Append(Environment.NewLine);
                        }
                        else
                        {
                            sb.Append(textBetweenElements.Trim());
                            singleLine = true;
                        }

                        i = nextStartElementPos - 1;
                    }
                    else
                    {
                        sb.Append(Environment.NewLine);
                    }
                }
            }
        }

        return sb.ToString();
    }

    /// <summary>
    ///     Pretty print given Json payload.
    /// </summary>
    /// <param name="payload"></param>
    /// <returns></returns>
    public static string PrettyPrintJson(string payload)
    {
        if ("{}".Equals(payload) || "[]".Equals(payload))
        {
            return payload;
        }

        var indentNum = 2;
        var indent = 0;
        var inQuote = false;
        var isKey = true;

        var s = payload.Trim();
        StringBuilder sb = new();
        var previousChar = '\0';
        foreach (var currentChar in s)
        {
            switch (currentChar)
            {
                case '"':
                    if (!inQuote && isKey)
                    {
                        sb.Append(' ', indent);
                    }

                    inQuote = !inQuote;
                    sb.Append(currentChar);
                    break;
                case ':':
                    if (inQuote)
                    {
                        sb.Append(currentChar);
                    }
                    else
                    {
                        isKey = false;
                        sb.Append(currentChar);
                        sb.Append(' ');
                    }

                    break;
                case ' ':
                    if (inQuote)
                    {
                        sb.Append(currentChar);
                    }

                    break;
                case '{':
                case '[':
                    if (inQuote)
                    {
                        sb.Append(currentChar);
                    }
                    else
                    {
                        if (isKey)
                        {
                            sb.Append(' ', indent);
                        }
                        else
                        {
                            isKey = true;
                        }

                        sb.Append(currentChar);
                        sb.Append(Environment.NewLine);
                        indent += indentNum;
                    }

                    break;
                case '}':
                case ']':
                    if (!inQuote)
                    {
                        if (previousChar == '"' || char.IsDigit(previousChar))
                        {
                            isKey = true;
                            sb.Append(Environment.NewLine);
                        }
                        else if (previousChar == '}' || previousChar == ']')
                        {
                            sb.Append(Environment.NewLine);
                        }

                        indent -= indentNum;
                        sb.Append(' ', indent);
                    }

                    sb.Append(currentChar);
                    break;
                case ',':
                    sb.Append(currentChar);
                    if (!inQuote)
                    {
                        isKey = true;
                        sb.Append(Environment.NewLine);
                    }

                    break;
                case '\r':
                case '\n':
                    break;
                default:
                    if (inQuote || Environment.NewLine != currentChar.ToString())
                    {
                        sb.Append(currentChar);
                    }

                    break;
            }

            if (inQuote || (Environment.NewLine != currentChar.ToString() && currentChar != ' '))
            {
                previousChar = currentChar;
            }
        }

        return sb.ToString();
    }
}
