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

namespace Agenix.Api.Exceptions;

/// <summary>
///     Represents an exception that is thrown when an action times out.
/// </summary>
public class ActionTimeoutException : AgenixSystemException
{
    protected readonly long Timeout;

    /// <summary>
    ///     Default constructor
    /// </summary>
    public ActionTimeoutException()
        : this(0L)
    {
    }

    /// <summary>
    ///     Default constructor
    /// </summary>
    public ActionTimeoutException(long timeout)
    {
        Timeout = timeout;
    }

    /// <summary>
    ///     Default constructor
    /// </summary>
    public ActionTimeoutException(long timeout, SystemException cause)
        : base(cause.Message)
    {
        Timeout = timeout;
    }

    /// <summary>
    ///     Overrides the Message property to provide a custom timeout message, indicating the duration of the timeout, if
    ///     specified.
    /// </summary>
    public override string Message => Timeout <= 0
        ? $"Action timeout. {GetDetailMessage()}".Trim()
        : $"Action timeout after {Timeout} milliseconds. {GetDetailMessage()}".Trim();

    /// <summary>
    ///     Provides a detailed message which can be overridden.
    /// </summary>
    /// <returns>A detailed message as a string.</returns>
    protected virtual string GetDetailMessage()
    {
        return string.Empty;
    }
}
