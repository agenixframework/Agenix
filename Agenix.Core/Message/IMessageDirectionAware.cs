namespace Agenix.Core.Message;

/// <summary>
///     Provides functionality to determine the direction of messages for a processor.
/// </summary>
public interface IMessageDirectionAware
{
    /// <summary>
    ///     Indicates the direction of messages this processor should apply to.
    /// </summary>
    /// <returns></returns>
    MessageDirection GetDirection();
}