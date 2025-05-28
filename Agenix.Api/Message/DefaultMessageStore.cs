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

using System.Collections.Concurrent;
using Agenix.Api.Endpoint;

namespace Agenix.Api.Message;

/// <summary>
///     Represents the default implementation of a message store that uses a concurrent dictionary
///     to store and retrieve messages identified by unique string keys.
/// </summary>
public class DefaultMessageStore : ConcurrentDictionary<string, IMessage>, IMessageStore
{
    /// <summary>
    ///     Retrieves a message from the store using a unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier for the message to be retrieved.</param>
    /// <returns>The message associated with the specified identifier, or null if no match is found.</returns>
    public IMessage GetMessage(string id)
    {
        TryGetValue(id, out var message);
        return message;
    }

    /// <summary>
    ///     Stores a message in the store using a unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier for the message to be stored.</param>
    /// <param name="message">The message instance to be stored in the store.</param>
    public void StoreMessage(string id, IMessage message)
    {
        TryAdd(id, message);
    }

    /// <summary>
    ///     Constructs a message name by combining the name of the test action and the name of the endpoint.
    /// </summary>
    /// <param name="action">The test action that provides the action name.</param>
    /// <param name="endpoint">The endpoint that provides the endpoint name.</param>
    /// <returns>A string representing the constructed message name in the format: "ActionName(EndpointName)".</returns>
    public string ConstructMessageName(ITestAction action, IEndpoint endpoint)
    {
        return $"{action.Name}({endpoint.Name})";
    }
}
