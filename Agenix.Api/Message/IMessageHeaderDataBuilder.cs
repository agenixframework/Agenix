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

namespace Agenix.Api.Message;

public delegate string MessageHeaderDataBuilder(TestContext context);

/// <summary>
///     Interface representing a builder for message header data.
/// </summary>
public interface IMessageHeaderDataBuilder : IMessageHeaderBuilder
{
    /// <summary>
    ///     Builds and returns header data from the provided test context.
    /// </summary>
    /// <param name="context">The context containing the necessary data for building the headers.</param>
    /// <returns>A dictionary representing the built header data.</returns>
    new Dictionary<string, object> BuilderHeaders(TestContext context)
    {
        return new Dictionary<string, object>();
    }

    /// <summary>
    ///     Builds and returns header data from the provided test context.
    /// </summary>
    /// <param name="context">The context containing the necessary data for building the header.</param>
    /// <returns>A string representing the built header data.</returns>
    string BuildHeaderData(TestContext context);
}
