using Agenix.Api.Context;

namespace Agenix.Api.Message;

public delegate object MessagePayloadBuilder(TestContext context);

/// <summary>
///     Provides an interface for building message payloads using a test context.
/// </summary>
public interface IMessagePayloadBuilder
{
    /// <summary>
    ///     Builds the payload for a message using the provided test context.
    /// </summary>
    /// <param name="context">The test context used for building the payload.</param>
    /// <returns>An object representing the built payload.</returns>
    object BuildPayload(TestContext context);

    /// <summary>
    ///     Provides an interface for building message payloads using a test context.
    /// </summary>
    /// <typeparam name="T">The type of the payload builder.</typeparam>
    /// <typeparam name="TB">The type of the builder itself.</typeparam>
    public interface IBuilder<out T, TB>
        where T : IMessagePayloadBuilder
        where TB : IBuilder<T, TB>
    {
        /// <summary>
        ///     Builds the payload for a message using the provided test context.
        /// </summary>
        /// <returns>An object representing the built payload.</returns>
        T Build();
    }
}