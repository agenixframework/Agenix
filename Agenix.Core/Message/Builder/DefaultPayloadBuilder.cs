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

using Agenix.Api.Context;
using Agenix.Api.Message;

namespace Agenix.Core.Message.Builder;

/// <summary>
///     Represents a builder class that constructs message payloads with dynamic content using the provided context.
/// </summary>
public class DefaultPayloadBuilder(object payload) : IMessagePayloadBuilder
{
    /// <summary>
    ///     Builds the payload based on the provided context, replacing any dynamic content in a string payload.
    /// </summary>
    /// <param name="context">The context that provides methods for replacing dynamic content in a string.</param>
    /// <returns>The processed payload object.</returns>
    public virtual object BuildPayload(TestContext context)
    {
        return payload switch
        {
            null => "",
            string payloadString => context.ReplaceDynamicContentInString(payloadString),
            _ => payload
        };
    }

    /// <summary>
    ///     Retrieves the payload object that is being built.
    /// </summary>
    /// <returns>The payload object.</returns>
    public object Payload => payload;
}
