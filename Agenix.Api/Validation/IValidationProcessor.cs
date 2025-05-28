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

namespace Agenix.Api.Validation;

public delegate void ValidationProcessor(IMessage message, TestContext context);

/// <summary>
///     Defines methods for validating and processing messages within a specified test context.
/// </summary>
public interface IValidationProcessor : IMessageProcessor
{
    /// <summary>
    ///     Processes the provided message within the given context.
    /// </summary>
    /// <param name="message">The message to be processed.</param>
    /// <param name="context">The context within which the message will be processed.</param>
    void IMessageProcessor.Process(IMessage message, TestContext context)
    {
        Validate(message, context);
    }

    /// <summary>
    ///     Validates the provided message within the specified test context.
    /// </summary>
    /// <param name="message">The message to be validated.</param>
    /// <param name="context">The test context in which validation will occur.</param>
    void Validate(IMessage message, TestContext context);
}
