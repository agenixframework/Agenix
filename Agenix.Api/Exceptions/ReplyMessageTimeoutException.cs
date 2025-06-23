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
///     Exception thrown when a synchronous reply message times out.
/// </summary>
public class ReplyMessageTimeoutException : MessageTimeoutException
{
    /// <summary>
    ///     Exception thrown when a synchronous reply message times out.
    /// </summary>
    public ReplyMessageTimeoutException(long timeout, string endpoint)
        : base(timeout, endpoint)
    {
    }

    /// <summary>
    ///     Exception thrown when a synchronous reply message times out.
    /// </summary>
    public ReplyMessageTimeoutException(long timeout, string endpoint, SystemException cause)
        : base(timeout, endpoint, cause)
    {
    }

    /// <summary>
    ///     Gets an error message describing the reason for the failure to receive a synchronous reply message.
    /// </summary>
    /// <remarks>
    ///     If the Timeout property is less than or equal to zero and the Endpoint property is not set,
    ///     this property returns a default error message.
    ///     Otherwise, it includes the endpoint information in the error message.
    /// </remarks>
    public override string Message
    {
        get
        {
            if (Timeout <= 0 && Endpoint == null)
            {
                return "Failed to receive synchronous reply message.";
            }

            return $"Failed to receive synchronous reply message on endpoint: '{Endpoint}'";
        }
    }
}
