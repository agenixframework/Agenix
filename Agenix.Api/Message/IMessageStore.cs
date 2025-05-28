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

using Agenix.Api.Endpoint;

namespace Agenix.Api.Message;

/// <summary>
///     Defines a contract for managing messages, allowing retrieval, storage, and construction
///     of message names based on contextual components.
/// </summary>
public interface IMessageStore
{
    /// <summary>
    ///     Retrieves a message from the store using the specified identifier.
    /// </summary>
    /// <param name="id">The identifier of the message to be retrieved.</param>
    /// <returns>The message associated with the provided identifier, or null if no message is found.</returns>
    IMessage GetMessage(string id);

    /// <summary>
    ///     Stores a message in the store using the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier to associate with the message.</param>
    /// <param name="message">The message to be stored in the store.</param>
    void StoreMessage(string id, IMessage message);

    /// <summary>
    ///     Constructs a message name based on the provided action and endpoint.
    /// </summary>
    /// <param name="action">The test action that contributes to the message name.</param>
    /// <param name="endpoint">The endpoint that contributes to the message name.</param>
    /// <returns>A string representing the constructed message name composed of the action and endpoint details.</returns>
    string ConstructMessageName(ITestAction action, IEndpoint endpoint);
}
