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