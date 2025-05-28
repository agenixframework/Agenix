#region Imports

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
using Agenix.Api.Util;

#endregion

namespace Agenix.Api.IO;

/// <summary>
///     A <see cref="IResource" /> adapter implementation encapsulating a simple string.
/// </summary>
public class StringResource : AbstractResource
{
    /// <summary>
    ///     Creates a new instance of the <see cref="StringResource" /> class.
    /// </summary>
    public StringResource(string content)
        : this(content, Encoding.Default, null)
    {
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="StringResource" /> class.
    /// </summary>
    public StringResource(string content, Encoding encoding)
        : this(content, encoding, null)
    {
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="StringResource" /> class.
    /// </summary>
    public StringResource(string content, Encoding encoding, string description)
    {
        AssertUtils.ArgumentNotNull(encoding, "encoding");

        Content = content == null ? string.Empty : content;
        Encoding = encoding;
        _description = description == null ? string.Empty : description;
    }

    /// <summary>
    ///     Get the <see cref="System.IO.Stream" /> to
    ///     for accessing this resource.
    /// </summary>
    public override Stream InputStream => new MemoryStream(Encoding.GetBytes(Content), false);

    /// <summary>
    ///     Returns a description for this resource.
    /// </summary>
    /// <value>
    ///     A description for this resource.
    /// </value>
    /// <seealso cref="IResource.Description" />
    public override string Description => _description;

    /// <summary>
    ///     This implementation always returns true
    /// </summary>
    public override bool IsOpen => true;

    /// <summary>
    ///     This implemementation always returns true
    /// </summary>
    public override bool Exists => true;

    /// <summary>
    ///     Gets the encoding used to create a byte stream of the <see cref="Content" /> string.
    /// </summary>
    public Encoding Encoding { get; }

    /// <summary>
    ///     Gets the content encapsulated by this <see cref="StringResource" />.
    /// </summary>
    public string Content { get; }

    #region Fields

    private readonly string _description;

    #endregion
}
