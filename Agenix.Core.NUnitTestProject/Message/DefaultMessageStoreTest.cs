using Agenix.Core.Message;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Message;

[TestFixture]
public class DefaultMessageStoreTest
{
    private readonly IMessageStore messageStore = new DefaultMessageStore();

    [Test]
    public void TestStoreAndGetMessage()
    {
        messageStore.StoreMessage("request", new DefaultMessage("RequestMessage"));
        ClassicAssert.AreEqual(messageStore.GetMessage("request").GetPayload<string>(), "RequestMessage");
        ClassicAssert.IsNull(messageStore.GetMessage("unknown"));
    }
}