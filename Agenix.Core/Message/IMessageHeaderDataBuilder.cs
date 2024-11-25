using System.Collections.Generic;

namespace Agenix.Core.Message;

public delegate string MessageHeaderDataBuilder(TestContext context);

/// <summary>
///     Interface representing a builder for message header data.
/// </summary>
public interface IMessageHeaderDataBuilder : IMessageHeaderBuilder
{
    /// <summary>
    ///     Builds and returns header data from the provided test context.
    /// </summary>
    /// <param name="context">The context containing the necessary data for building the headers.</param>
    /// <returns>A dictionary representing the built header data.</returns>
    new Dictionary<string, object> BuilderHeaders(TestContext context)
    {
        return new Dictionary<string, object>();
    }

    /// <summary>
    ///     Builds and returns header data from the provided test context.
    /// </summary>
    /// <param name="context">The context containing the necessary data for building the header.</param>
    /// <returns>A string representing the built header data.</returns>
    string BuildHeaderData(TestContext context);
}