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

using Agenix.Api;
using Agenix.Api.Annotations;
using Agenix.Api.Context;
using Agenix.Api.Endpoint;
using Agenix.Api.Message;
using Agenix.Api.Spi;
using Agenix.Core.Endpoint.Direct;
using Agenix.Core.Message;
using Reqnroll;
using static Agenix.Core.Actions.ReceiveMessageAction.Builder;
using static Agenix.Core.Actions.SendMessageAction.Builder;

[Binding]
[AgenixConfiguration(Classes = [typeof(Endpoints)])]
public class EchoSteps
{
    [AgenixEndpoint(Name = "EchoEndpoint")]
    private IEndpoint _directEndpoint;

    [AgenixResource] protected TestContext context;

    [AgenixResource] protected ITestCaseRunner runner;

    [Given(@"^My name is (.*)$")]
    public void MyNameIs(string name)
    {
        context.SetVariable("username", name);
    }

    [When(@"^I say hello.*")]
    public void SayHello()
    {
        runner.When(Send("EchoEndpoint")
            .Name("Send message to echo endpoint")
            .Message()
            .Type(MessageType.PLAINTEXT)
            .Body("Hello, my name is ${username}!"));
    }

    [When(@"^I say goodbye.*")]
    public void SayGoodbye()
    {
        runner.When(Send("EchoEndpoint")
            .Message()
            .Type(MessageType.PLAINTEXT)
            .Body("Goodbye from ${username}!"));
    }

    [Then(@"^the service should return: ""([^""]*)""$")]
    public void VerifyReturn(string body)
    {
        runner.Then(Receive("EchoEndpoint")
            .Message()
            .Type(MessageType.PLAINTEXT)
            .Body(body));
    }

    private class Endpoints
    {
        [BindToRegistry] private readonly IMessageQueue _messages = new DefaultMessageQueue("messages");

        [BindToRegistry(Name = "EchoEndpoint")]
        private IEndpoint _directEndpoint = new DirectEndpointBuilder()
            .Queue("TEST.direct.queue")
            .Build();

        [BindToRegistry]
        public DirectEndpoint Foo()
        {
            return new DirectEndpointBuilder()
                .Queue(_messages)
                .Build();
        }

        [BindToRegistry(Name = "TEST.direct.queue")]
        private IMessageQueue Queue()
        {
            return new DefaultMessageQueue("FOO.direct.queue");
        }
    }
}
