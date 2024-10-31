using System.Collections.Concurrent;
using Agenix.Core.Endpoint;

namespace Agenix.Core.Message;

public class DefaultMessageStore : ConcurrentDictionary<string, IMessage>, IMessageStore
{
    public IMessage GetMessage(string id)
    {
        TryGetValue(id, out var message);
        return message;
    }

    public void StoreMessage(string id, IMessage message)
    {
        TryAdd(id, message);
    }

    public string ConstructMessageName(ITestAction action, IEndpoint endpoint)
    {
        return string.Format("{0}({1})", action.Name, endpoint.Name);
    }
}