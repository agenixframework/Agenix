#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
//
// Copyright (c) 2025 Agenix
//
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

#endregion

using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Agenix.Api;
using Agenix.Api.Annotations;
using Agenix.Api.Condition;
using Agenix.Api.Message;
using Agenix.Core.Actions;
using Agenix.Core.Tests.Util;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using NUnit.Framework;
using static Agenix.Core.Actions.ReceiveMessageAction.Builder;
using static Agenix.Core.Actions.SendMessageAction.Builder;
using static Agenix.Core.Actions.SleepAction.Builder;
using static Agenix.Core.Container.Parallel.Builder;
using static Agenix.Core.Container.Sequence.Builder;
using static Agenix.Core.Container.Wait.Builder<Agenix.Api.Condition.ICondition>;

namespace Agenix.Core.Tests.NUnitIntegration.Container;

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
        {
            return;
        }

        try
        {
            var context = _server.EndGetContext(result);
            context.Response.StatusCode = 200;
            context.Response.Close();

            _server.BeginGetContext(OnRequest, null);
        }
        catch (HttpListenerException ex)
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
        var actionStarted = new ManualResetEventSlim(false);
        var actionCompleted = new ManualResetEventSlim(false);
        var sleepStarted = new ManualResetEventSlim(false);
        var sleepCompleted = new ManualResetEventSlim(false);

        // Create a controlled sleep action
        var controlledSleepAction = DefaultTestActionBuilder.Action(context =>
        {
            actionStarted.Set();
            sleepStarted.Set();
            Thread.Sleep(250);
            sleepCompleted.Set();
            actionCompleted.Set();
        }).Name("ControlledSleepAction").Description("A controlled sleep action that can be used for timing tests");

        _gherkin.When(WaitFor<ICondition>()
            .Execution()
            .Interval(400)
            .Milliseconds(2000)
            .Action(controlledSleepAction)
        );

        // Verify the timing expectations
        Assert.That(actionStarted.Wait(TimeSpan.FromSeconds(2)), Is.True,
            "Action should have started");

        Assert.That(sleepStarted.Wait(TimeSpan.FromSeconds(1)), Is.True,
            "Sleep should have started");

        Assert.That(sleepCompleted.Wait(TimeSpan.FromSeconds(3)), Is.True,
            "Sleep should have completed");

        Assert.That(actionCompleted.Wait(TimeSpan.FromSeconds(1)), Is.True,
            "Action should have completed");
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
