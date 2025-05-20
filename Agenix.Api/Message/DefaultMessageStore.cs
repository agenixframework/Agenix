using System.Collections.Concurrent;
using Agenix.Api.Endpoint;

namespace Agenix.Api.Message;

/// <summary>
/// Represents the default implementation of a message store that uses a concurrent dictionary
/// to store and retrieve messages identified by unique string keys.
/// </summary>
public class DefaultMessageStore : ConcurrentDictionary<string, IMessage>, IMessageStore
{
    /// <summary>
    /// Retrieves a message from the store using a unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier for the message to be retrieved.</param>
    /// <returns>The message associated with the specified identifier, or null if no match is found.</returns>
    public IMessage GetMessage(string id)
    {
        TryGetValue(id, out var message);
        return message;
    }

    /// <summary>
    /// Stores a message in the store using a unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier for the message to be stored.</param>
    /// <param name="message">The message instance to be stored in the store.</param>
    public void StoreMessage(string id, IMessage message)
    {
        TryAdd(id, message);
    }

    /// <summary>
    /// Constructs a message name by combining the name of the test action and the name of the endpoint.
    /// </summary>
    /// <param name="action">The test action that provides the action name.</param>
    /// <param name="endpoint">The endpoint that provides the endpoint name.</param>
    /// <returns>A string representing the constructed message name in the format: "ActionName(EndpointName)".</returns>
    public string ConstructMessageName(ITestAction action, IEndpoint endpoint)
    {
        return $"{action.Name}({endpoint.Name})";
    }
}