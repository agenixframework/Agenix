using Agenix.Core.Endpoint;

namespace Agenix.Core.Message;

public interface IMessageStore
{
    IMessage GetMessage(string id);

    void StoreMessage(string id, IMessage message);

    string ConstructMessageName(ITestAction action, IEndpoint endpoint);
}