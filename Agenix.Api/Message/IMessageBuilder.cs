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

/// <summary>
///     Interface representing a builder for constructing messages.
/// </summary>
public interface IMessageBuilder
{
    IMessage Build(TestContext context, string messageType);
}

/// <summary>
///     Define a delegate for creating <see cref="IMessage" /> objects based on the provided
///     <see cref="TestContext" /> and message type.
/// </summary>
/// <param name="context">
///     Instance of <see cref="TestContext" /> which provides utility methods and state
///     for generating messages.
/// </param>
/// <param name="messageType">A string indicating the type of message to be created.</param>
/// <returns>An instance of <see cref="IMessage" /> based on the specified context and message type.</returns>
public delegate IMessage MessageBuilder(TestContext context, string messageType);
