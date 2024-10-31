using System.Collections.Generic;

namespace Agenix.Core.Message;

/// Represents a builder interface for handling message headers.
/// /
public interface IWithHeaderBuilder
{
    /**
     * Add message header builder.
     * @param builder
     */
    void AddHeaderBuilder(IMessageHeaderBuilder builder);

    /**
     * Gets the list of header builders.
     * @return
     */
    List<IMessageHeaderBuilder> GetHeaderBuilders();
}