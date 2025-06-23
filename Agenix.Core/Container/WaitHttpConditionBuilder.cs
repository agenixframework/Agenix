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

using Agenix.Core.Condition;

namespace Agenix.Core.Container;

/// Builds a wait condition specifically designed for HTTP checks, allowing for customization
/// of HTTP-related parameters such as URL, timeout, status, and method for defining conditions
/// that involve HTTP requests.
public class WaitHttpConditionBuilder : WaitConditionBuilder<HttpCondition, WaitHttpConditionBuilder>
{
    /// Constructs a WaitHttpConditionBuilder with a specified builder.
    /// @param builder the builder used to initialize the WaitHttpConditionBuilder
    /// /
    public WaitHttpConditionBuilder(Wait.Builder<HttpCondition> builder) : base(builder)
    {
    }

    /// Specifies the URL to be used in the HTTP condition.
    /// @param requestUrl The URL to set for the HTTP condition.
    /// @return The current instance of WaitHttpConditionBuilder for method chaining.
    /// /
    public WaitHttpConditionBuilder Url(string requestUrl)
    {
        GetCondition().Url = requestUrl;
        return this;
    }

    /// Sets the HTTP connection timeout.
    /// @param timeout The timeout duration to be set for the HTTP connection as a string.
    /// @return The current instance of WaitHttpConditionBuilder for method chaining.
    /// /
    public WaitHttpConditionBuilder Timeout(string timeout)
    {
        GetCondition().Timeout = timeout;
        return this;
    }

    /// Sets the HTTP connection timeout using a long value.
    /// @param timeout The timeout value in milliseconds to be set for the HTTP connection.
    /// @return The current instance of WaitHttpConditionBuilder for method chaining.
    /// /
    public WaitHttpConditionBuilder Timeout(long timeout)
    {
        GetCondition().Timeout = timeout.ToString();
        return this;
    }

    /// Sets the HTTP status code to check.
    /// @param status The HTTP status code to be set in the condition.
    /// @return The current instance of WaitHttpConditionBuilder for method chaining.
    /// /
    public WaitHttpConditionBuilder Status(int status)
    {
        GetCondition().HttpResponseCode = status.ToString();
        return this;
    }

    /// Sets the URL for the HTTP request condition.
    /// @param requestUrl The URL to be set in the HTTP condition.
    /// @return The current instance of WaitHttpConditionBuilder for method chaining.
    /// /
    public WaitHttpConditionBuilder Method(string method)
    {
        GetCondition().Method = method.ToUpper();
        return this;
    }
}
