using System;
using System.Collections.Generic;
using System.Net;
using Agenix.Core.Annotations;
using Agenix.Core.Condition;
using Agenix.Core.Message;
using Agenix.Core.NUnitTestProject.Util;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using NUnit.Framework;
using static Agenix.Core.Container.Parallel.Builder;
using static Agenix.Core.Actions.SleepAction.Builder;
using static Agenix.Core.Container.Sequence.Builder;
using static Agenix.Core.Actions.ReceiveMessageAction.Builder;
using static Agenix.Core.Actions.SendMessageAction.Builder;
using static Agenix.Core.Container.Wait.Builder<Agenix.Core.Condition.ICondition>;

namespace Agenix.Core.NUnitTestProject.NUnitIntegration.Container;

[NUnitAgenixSupport]
public class WaitIT
{
    private const int ServerPort = 8000;

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    [AgenixResource] private IGherkinTestActionRunner _gherkin;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value
    private HttpListener _server;

    [OneTimeSetUp]
    public void StartServer()
    {
        _server = new HttpListener();
        _server.Prefixes.Add($"http://localhost:{ServerPort}/test/");
        _server.Start();
        _server.BeginGetContext(OnRequest, _server);
    }

    private void OnRequest(IAsyncResult result)
    {
        if (!_server.IsListening)
            return;

        var context = _server.EndGetContext(result);
        context.Response.StatusCode = 200;
        context.Response.Close();

        _server.BeginGetContext(OnRequest, _server);
    }

    [OneTimeTearDown]
    public void StopServer()
    {
        if (_server is { IsListening: true })
        {
            _server.Stop();
            _server.Close();
        }
    }

    [Test]
    public void WaitHttp()
    {
        _gherkin.When(WaitFor<ICondition>()
            .Http()
            .Url($"http://localhost:{ServerPort}/test"));
    }

    [Test]
    public void WaitMessage()
    {
        const string messageName = "myTestMessage";

        _gherkin.When(Parallel().Actions(
            Sequential().Actions(
                WaitFor<ICondition>()
                    .Message()
                    .Name(messageName)
            ),
            Sequential().Actions(
                Send("direct:waitQueue")
                    .Message()
                    .Name(messageName)
                    .Body("Wait for me")
                    .Header("Operation", "waitForMe")),
            Receive("direct:waitQueue")
                .Selector(new Dictionary<string, string> { { "Operation", "waitForMe" } })
                .Message()
                .Type(MessageType.PLAINTEXT)
                .Name(messageName)
                .Body("Wait for me")
                .Header("Operation", "waitForMe")
        ));
    }

    [Test]
    public void WaitAction()
    {
        _gherkin.When(WaitFor<ICondition>()
            .Execution()
            .Interval(300)
            .Milliseconds(500)
            .Action(Sleep().Milliseconds(250))
        );
    }

    [Test]
    public void WaitForFileUsingResource()
    {
        var file = IFileHelper.CreateTmpFile();

        _gherkin.When(WaitFor<ICondition>()
            .File()
            .Resource(file));
    }

    [Test]
    public void WaitForFileUsingPath()
    {
        var file = IFileHelper.CreateTmpFile();

        _gherkin.When(WaitFor<ICondition>()
            .File()
            .Path(new Uri(file.FullName).AbsolutePath));
    }
}