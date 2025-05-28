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

using System.Configuration.Internal;

namespace Agenix.Api.Util;

/// <summary>
///     Holds text position information for e.g. error reporting purposes.
/// </summary>
/// <seealso cref="ConfigXmlElement" />
/// <seealso cref="ConfigXmlAttribute" />
public interface ITextPosition : IConfigErrorInfo
{
    /// <summary>
    ///     Gets a string specifying the file/resource name related to the configuration details.
    /// </summary>
    new string Filename { get; }

    /// <summary>
    ///     Gets an integer specifying the line number related to the configuration details.
    /// </summary>
    new int LineNumber { get; }

    /// <summary>
    ///     Gets an integer specifying the line position related to the configuration details.
    /// </summary>
    int LinePosition { get; }
}
