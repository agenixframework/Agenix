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

using Agenix.Api.Message;

namespace Agenix.Core.Endpoint.Direct;

/// <summary>
///     DirectEndpointConfiguration manages configuration settings specifically
///     for direct endpoints, such as setting and getting the message queue
///     and queue name.
/// </summary>
public class DirectEndpointConfiguration : AbstractEndpointConfiguration
{
    /// <summary>
    ///     Destination queue.
    /// </summary>
    private IMessageQueue _queue;

    /// <summary>
    ///     Destination queue name.
    /// </summary>
    private string _queueName;

    /// <summary>
    ///     Set the message queue.
    /// </summary>
    /// <param name="queue">The queue to set.</param>
    public void SetQueue(IMessageQueue queue)
    {
        _queue = queue;
    }

    /// <summary>
    ///     Sets the destination queue name.
    /// </summary>
    /// <param name="queueName">The queue name to set.</param>
    public void SetQueueName(string queueName)
    {
        _queueName = queueName;
    }

    /// <summary>
    ///     Gets the queue.
    /// </summary>
    /// <returns>The queue.</returns>
    public IMessageQueue GetQueue()
    {
        return _queue;
    }

    /// <summary>
    ///     Gets the queue name.
    /// </summary>
    /// <returns>The queue name.</returns>
    public string GetQueueName()
    {
        return _queueName;
    }
}
