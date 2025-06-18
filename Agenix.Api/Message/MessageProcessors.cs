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

using System.Collections.ObjectModel;
using Agenix.Api.Exceptions;
using Agenix.Core;

namespace Agenix.Api.Message;

/// <summary>
///     Provides operations to manage and process a collection of message processors.
/// </summary>
public class MessageProcessors
{
    private List<IMessageProcessor> _messageProcessors = [];

    /// <summary>
    ///     Sets the messageProcessors property.
    /// </summary>
    /// <param name="messageProcessors">List of message processors.</param>
    public void SetMessageProcessors(List<IMessageProcessor> messageProcessors)
    {
        _messageProcessors = messageProcessors;
    }

    /// <summary>
    ///     Gets the message processors.
    /// </summary>
    /// <returns>Unmodifiable list of message processors.</returns>
    public IReadOnlyList<IMessageProcessor> GetMessageProcessors()
    {
        return new ReadOnlyCollection<IMessageProcessor>(_messageProcessors);
    }

    /// <summary>
    ///     Adds a new message processor.
    /// </summary>
    /// <param name="processor">Message processor to add.</param>
    public void AddMessageProcessor(IMessageProcessor processor)
    {
        if (processor is IScoped scopedProcessor && !scopedProcessor.IsGlobalScope())
        {
            throw new AgenixSystemException(
                "Unable to add non-global scoped processor to global message processors - " +
                "either declare processor as global scope or explicitly add it to test actions instead");
        }

        _messageProcessors.Add(processor);
    }
}
