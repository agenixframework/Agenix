using Agenix.Api.Message;

namespace Agenix.Core.Message;

/// <summary>
///     A message selector that accepts all messages on the queue.
/// </summary>
public class AllAcceptingMessageSelector : IMessageSelector
{
    /// <summary>
    ///     Special message selector accepts all messages on queue.
    /// </summary>
    /// <param name="message"></param>
    /// <returns></returns>
    public bool Accept(IMessage message)
    {
        return true;
    }
}