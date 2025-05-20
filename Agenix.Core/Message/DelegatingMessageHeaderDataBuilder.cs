using System.Collections.Generic;
using Agenix.Api.Context;
using Agenix.Api.Message;

namespace Agenix.Core.Message;

/// <summary>
///     Delegates the building of message payloads to another specified message payload builder.
/// </summary>
/// <param name="builder">The message payload builder to which the building process is delegated.</param>
public class DelegatingMessageHeaderDataBuilder(MessageHeaderDataBuilder builder) : IMessageHeaderDataBuilder
{
    /// <summary>
    /// Builds the header data for a message using the provided context.
    /// </summary>
    /// <param name="context">The test context containing necessary data for building the header.</param>
    /// <returns>A string representing the built header data.</returns>
    public string BuildHeaderData(TestContext context)
    {
        return builder(context);
    }

    /// <summary>
    /// Builds a dictionary of headers using the provided test context.
    /// </summary>
    /// <param name="context">The test context containing the necessary data for constructing the headers.</param>
    /// <returns>A dictionary where the keys are header names and the values are header data.</returns>
    public Dictionary<string, object> BuilderHeaders(TestContext context)
    {
        return new Dictionary<string, object>();
    }
}