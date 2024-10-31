using System.Collections.Generic;

namespace Agenix.Core.Message;

/**
 * Build headers for a message.
 */
public interface IMessageHeaderBuilder
{
    /**
     * @param context the current test context.
     */
    Dictionary<string, object> BuilderHeaders(TestContext context);
}