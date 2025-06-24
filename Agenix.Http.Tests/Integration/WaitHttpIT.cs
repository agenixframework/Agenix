using Agenix.Api.Annotations;
using Agenix.Api.Condition;
using Agenix.Api.Spi;
using Agenix.Core;
using Agenix.Http.Client;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using static Agenix.Core.Actions.ReceiveMessageAction.Builder;
using static Agenix.Core.Actions.SendMessageAction.Builder;
using static Agenix.Core.Container.Wait.Builder<Agenix.Api.Condition.ICondition>;
using HttpClient = Agenix.Http.Client.HttpClient;

namespace Agenix.Http.Tests.Integration;

[NUnitAgenixSupport]
public class WaitHttpIT
{
    private const string fakeApiRequestUrl = "https://jsonplaceholder.typicode.com/posts/1";

    [BindToRegistry(Name = "_client")]
    private readonly HttpClient _client = new HttpClientBuilder()
        .RequestUrl(fakeApiRequestUrl)
        .RequestMethod(HttpMethod.Get)
        .Build();

    [AgenixResource]
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private IGherkinTestActionRunner gherkin;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [Test]
    public void WaitHttpAsAction()
    {
        gherkin.When(WaitFor<ICondition>()
            .Execution()
            .Action(Send(_client)));

        gherkin.Then(Receive(_client));
    }

    [Test]
    public void WaitHttpAsActionWithReferenceClient()
    {
        gherkin.When(WaitFor<ICondition>()
            .Execution()
            .Action(Send("_client")));

        gherkin.Then(Receive("_client"));
    }

    [Test]
    public void WaitHttp()
    {
        gherkin.When(WaitFor<ICondition>()
            .Http()
            .Url(fakeApiRequestUrl));
    }
}
