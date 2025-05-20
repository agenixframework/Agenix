using System;
using System.Collections.Generic;
using System.Net;
using Agenix.Api.Annotations;
using Agenix.Api.Condition;
using Agenix.Api.Message;
using Agenix.Core.NUnitTestProject.Util;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using NUnit.Framework;
using static Agenix.Core.Container.Parallel.Builder;
using static Agenix.Core.Actions.SleepAction.Builder;
using static Agenix.Core.Container.Sequence.Builder;
using static Agenix.Core.Actions.ReceiveMessageAction.Builder;
using static Agenix.Core.Actions.SendMessageAction.Builder;
using static Agenix.Core.Container.Wait.Builder<Agenix.Api.Condition.ICondition>;

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
        _server.BeginGetContext(OnRequest, null);
    }

    private void OnRequest(IAsyncResult result)
    {
        if (result == null || !_server.IsListening)
            return;

        try
        {
            var context = _server.EndGetContext(result);
            context.Response.StatusCode = 200;
            context.Response.Close();

            _server.BeginGetContext(OnRequest, null);
        } catch (HttpListenerException ex)
        {
            Console.WriteLine($"HttpListener exception: {ex.Message}");
        }
        catch (ObjectDisposedException)
        {
            Console.WriteLine("Listener has been disposed.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Unhandled exception: {ex}");
        }

    }

    [OneTimeTearDown]
    public void StopServer()
    {
        try
        {
            if (_server is { IsListening: true })
            {
                _server.Stop();
            }
            _server.Close();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error stopping listener: {ex.Message}");
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
                .Selector(new Dictionary<string, object> { { "Operation", "waitForMe" } })
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