using Agenix.Api.Endpoint;

namespace Agenix.Api.Message;

/// <summary>
/// Defines a contract for managing messages, allowing retrieval, storage, and construction
/// of message names based on contextual components.
/// </summary>
public interface IMessageStore
{
    /// <summary>
    /// Retrieves a message from the store using the specified identifier.
    /// </summary>
    /// <param name="id">The identifier of the message to be retrieved.</param>
    /// <returns>The message associated with the provided identifier, or null if no message is found.</returns>
    IMessage GetMessage(string id);

    /// <summary>
    /// Stores a message in the store using the specified identifier.
    /// </summary>
    /// <param name="id">The unique identifier to associate with the message.</param>
    /// <param name="message">The message to be stored in the store.</param>
    void StoreMessage(string id, IMessage message);

    /// <summary>
    /// Constructs a message name based on the provided action and endpoint.
    /// </summary>
    /// <param name="action">The test action that contributes to the message name.</param>
    /// <param name="endpoint">The endpoint that contributes to the message name.</param>
    /// <returns>A string representing the constructed message name composed of the action and endpoint details.</returns>
    string ConstructMessageName(ITestAction action, IEndpoint endpoint);
}