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

#endregion

namespace Agenix.Api.IO;

/// <summary>
///     Simple interface for objects that are sources for
///     <see cref="System.IO.Stream" />s.
/// </summary>
/// <seealso cref="IResource" />
public interface IInputStreamSource
{
    /// <summary>
    ///     Return an <see cref="System.IO.Stream" /> for this resource.
    /// </summary>
    /// <remarks>
    ///     <note type="caution">
    ///         Clients of this interface must be aware that every access of this
    ///         property will create a <i>fresh</i> <see cref="System.IO.Stream" />;
    ///         it is the responsibility of the calling code to close any such
    ///         <see cref="System.IO.Stream" />.
    ///     </note>
    /// </remarks>
    /// <value>
    ///     An <see cref="System.IO.Stream" />.
    /// </value>
    /// <exception cref="System.IO.IOException">
    ///     If the stream could not be opened.
    /// </exception>
    Stream InputStream { get; }
}
