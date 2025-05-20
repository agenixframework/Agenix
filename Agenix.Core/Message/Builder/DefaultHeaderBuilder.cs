using System.Collections.Generic;
using Agenix.Api.Context;
using Agenix.Api.Message;

namespace Agenix.Core.Message.Builder;

/// <summary>
///     Responsible for building message headers using a provided set of header key-value pairs.
/// </summary>
public class DefaultHeaderBuilder(Dictionary<string, object> headers) : IMessageHeaderBuilder
{
    /// <summary>
    ///     Builds message headers by resolving dynamic values within the headers using the provided context.
    /// </summary>
    /// <param name="context">The context used to resolve dynamic values within the headers.</param>
    /// <returns>A dictionary containing the resolved message headers.</returns>
    public Dictionary<string, object> BuilderHeaders(TestContext context)
    {
        return context.ResolveDynamicValuesInMap(headers);
    }
}