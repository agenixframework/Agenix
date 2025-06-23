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
using Agenix.Api.Variable;

namespace Agenix.Core.Message;

/// <summary>
///     Extracts variables from a message by delegating the extraction work to another specified variable extractor.
/// </summary>
/// <param name="variableExtractor">The variable extractor to which the extraction work is delegated.</param>
public class DelegatingVariableExtractor(VariableExtractor variableExtractor) : IVariableExtractor
{
    /// <summary>
    ///     Extracts variables from a message and context using the provided variable extractor delegate.
    /// </summary>
    /// <param name="message">The message from which variables are to be extracted.</param>
    /// <param name="context">The context used during the variable extraction process.</param>
    public void ExtractVariables(IMessage message, TestContext context)
    {
        variableExtractor(message, context);
    }

    /// <summary>
    ///     Processes the given message within the provided test context by extracting variables.
    /// </summary>
    /// <param name="message">The message to be processed.</param>
    /// <param name="context">The test context in which the message processing occurs.</param>
    public void Process(IMessage message, TestContext context)
    {
        ExtractVariables(message, context);
    }
}
