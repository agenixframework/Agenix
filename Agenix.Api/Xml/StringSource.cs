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

namespace Agenix.Api.Xml;

/// <summary>
///     A simple stream source representation of a static String content. Can be read many times and uses default encoding
///     set via configuration settings.
/// </summary>
public class StringSource
{
    /// <summary>
    ///     Constructor using source content as String.
    /// </summary>
    /// <param name="content">the content</param>
    public StringSource(string content)
        : this(content, AgenixSettings.AgenixFileEncoding())
    {
    }

    /// <summary>
    ///     Constructor using source content as String and encoding.
    /// </summary>
    /// <param name="content">the content</param>
    /// <param name="encoding">the encoding</param>
    public StringSource(string content, Encoding encoding)
    {
        Content = content;
        Encoding = encoding;
    }

    /// <summary>
    ///     Constructor using source content as String and encoding name.
    /// </summary>
    /// <param name="content">the content</param>
    /// <param name="encodingName">the encoding name</param>
    public StringSource(string content, string encodingName)
    {
        Content = content;
        Encoding = Encoding.GetEncoding(encodingName);
    }

    /// <summary>
    ///     Obtains the content.
    /// </summary>
    /// <returns>the content</returns>
    public string Content { get; }

    /// <summary>
    ///     Obtains the encoding.
    /// </summary>
    /// <returns>the encoding</returns>
    public Encoding Encoding { get; }

    /// <summary>
    ///     Obtains the encoding name.
    /// </summary>
    /// <returns>the encoding name</returns>
    public string EncodingName => Encoding.WebName;

    public TextReader GetReader()
    {
        return new StringReader(Content);
    }

    public Stream GetInputStream()
    {
        return new MemoryStream(Encoding.GetBytes(Content));
    }

    public override string ToString()
    {
        return Content;
    }
}
