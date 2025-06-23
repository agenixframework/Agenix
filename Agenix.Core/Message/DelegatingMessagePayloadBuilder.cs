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

namespace Agenix.Core.Message;

/// <summary>
///     Delegates the building of message payloads to another specified message payload builder.
/// </summary>
/// <param name="builder">The message payload builder to which the building process is delegated.</param>
public class DelegatingMessagePayloadBuilder(MessagePayloadBuilder builder) : IMessagePayloadBuilder
{
    /// <summary>
    ///     Builds the payload for a message using the provided TestContext by delegating to the specified message payload
    ///     builder.
    /// </summary>
    /// <param name="context">The context of the test execution, providing relevant variables, functions, and configurations.</param>
    /// <returns>The constructed payload object as created by the delegated message payload builder.</returns>
    public object BuildPayload(TestContext context)
    {
        return builder(context);
    }
}
