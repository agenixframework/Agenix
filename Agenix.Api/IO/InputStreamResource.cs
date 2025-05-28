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

using Agenix.Api.Util;

#endregion

namespace Agenix.Api.IO;

/// <summary>
///     <see cref="IResource" /> adapter implementation for a
///     <see cref="System.IO.Stream" />.
/// </summary>
/// <remarks>
///     <p>
///         Should only be used if no other <see cref="IResource" />
///         implementation is applicable.
///     </p>
///     <p>
///         In contrast to other <see cref="IResource" />
///         implementations, this is an adapter for an <i>already opened</i>
///         resource - the <see cref="IsOpen" />
///         therefore always returns <see langword="true" />. Do not use this class
///         if you need to keep the resource descriptor somewhere, or if you need
///         to read a stream multiple times.
///     </p>
/// </remarks>
public class InputStreamResource : AbstractResource
{
    #region Constructor (s) / Destructor

    /// <summary>
    ///     Creates a new instance of the
    ///     <see cref="InputStreamResource" /> class.
    /// </summary>
    /// <param name="inputStream">
    ///     The input <see cref="System.IO.Stream" /> to use.
    /// </param>
    /// <param name="description">
    ///     Where the input <see cref="System.IO.Stream" /> comes from.
    /// </param>
    /// <exception cref="System.ArgumentNullException">
    ///     If the supplied <paramref name="inputStream" /> is
    ///     <see langword="null" />.
    /// </exception>
    public InputStreamResource(Stream inputStream, string description)
    {
        AssertUtils.ArgumentNotNull(inputStream, "inputStream");

        _inputStream = inputStream;
        _description = description ?? string.Empty;
    }

    #endregion

    #region Properties

    /// <summary>
    ///     The input <see cref="System.IO.Stream" /> to use.
    /// </summary>
    /// <exception cref="System.InvalidOperationException">
    ///     If the underlying <see cref="System.IO.Stream" /> has already
    ///     been read.
    /// </exception>
    public override Stream InputStream
    {
        get
        {
            if (_inputStream == null)
                throw new InvalidOperationException(
                    "InputStream has already been read - " +
                    "do not use InputStreamResource if a stream " +
                    "needs to be read multiple times");
            var result = _inputStream;
            _inputStream = null;
            return result;
        }
    }

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
    ///     This implementation always returns true
    /// </summary>
    public override bool Exists => true;

    #endregion

    #region Fields

    private Stream _inputStream;
    private readonly string _description;

    #endregion
}
