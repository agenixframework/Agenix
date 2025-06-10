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

/// Interface for transforming a message within a specified test context.
public interface IMessageTransformer
{
    /// Transform a message with a given test context and return a new message.
    /// @param message the message to process.
    /// @param context the current test context.
    /// @return the processed message.
    /// /
    extern IMessage Transform(IMessage message, TestContext context);

    /// Defines a generic builder interface for constructing instances of IMessageTransformer.
    /// @type param T The type of IMessageTransformer that this builder will create.
    /// @type param TB The type of the builder itself, to allow for fluent builder patterns.
    /// /
    interface IBuilder<out T, TB> where T : IMessageTransformer
        where TB : IBuilder
    {
        /// Builds a new message processor instance.
        /// @return new instance of message processor.
        T Build();
    }
}
