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

#region Imports

using System.Runtime.Serialization;

#endregion

namespace Agenix.Api.Exceptions;

/// <summary>
///     Superclass for all exceptions thrown in the Objects namespace and sub-namespaces.
/// </summary>
[Serializable]
public class ReflectionException : ApplicationException
{
    #region Constructor (s) / Destructor

    /// <summary>Creates a new instance of the ObjectsException class.</summary>
    public ReflectionException()
    {
    }

    /// <summary>
    ///     Creates a new instance of the ObjectsException class. with the specified message.
    /// </summary>
    /// <param name="message">
    ///     A message about the exception.
    /// </param>
    public ReflectionException(string message) : base(message)
    {
    }

    /// <summary>
    ///     Creates a new instance of the ObjectsException class with the specified message
    ///     and root cause.
    /// </summary>
    /// <param name="message">
    ///     A message about the exception.
    /// </param>
    /// <param name="rootCause">
    ///     The root exception that is being wrapped.
    /// </param>
    public ReflectionException(string message, Exception rootCause)
        : base(message, rootCause)
    {
    }

    /// <summary>
    ///     Creates a new instance of the ObjectsException class.
    /// </summary>
    /// <param name="info">
    ///     The <see cref="System.Runtime.Serialization.SerializationInfo" />
    ///     that holds the serialized object data about the exception being thrown.
    /// </param>
    /// <param name="context">
    ///     The <see cref="System.Runtime.Serialization.StreamingContext" />
    ///     that contains contextual information about the source or destination.
    /// </param>
    protected ReflectionException(
        SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    #endregion
}
