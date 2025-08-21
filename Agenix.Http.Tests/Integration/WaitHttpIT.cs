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
using Agenix.Api.Condition;
using Agenix.Api.Spi;
using Agenix.Http.Client;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using static Agenix.Core.Actions.ReceiveMessageAction.Builder;
using static Agenix.Core.Actions.SendMessageAction.Builder;
using static Agenix.Core.Container.Wait.Builder<Agenix.Api.Condition.ICondition>;
using HttpClient = Agenix.Http.Client.HttpClient;

namespace Agenix.Http.Tests.Integration;

[NUnitAgenixSupport]
public class WaitHttpIt
{
    private const string FakeApiRequestUrl = "https://jsonplaceholder.typicode.com/posts/1";

    [BindToRegistry(Name = "_client")]
    private readonly HttpClient _client = new HttpClientBuilder()
        .RequestUrl(FakeApiRequestUrl)
        .RequestMethod(HttpMethod.Get)
        .Build();

    [AgenixResource]
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private IGherkinAsyncTestActionRunner gherkin;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [Test]
    public async Task WaitHttpAsAction()
    {
        await gherkin.When(WaitFor<ICondition>()
            .Execution()
            .Action(Send(_client)));

        await gherkin.Then(Receive(_client));
    }

    [Test]
    public async Task WaitHttpAsActionWithReferenceClient()
    {
        await gherkin.When(WaitFor<ICondition>()
            .Execution()
            .Action(Send("_client")));

        await gherkin.Then(Receive("_client"));
    }

    [Test]
    public async Task WaitHttp()
    {
        await gherkin.When(WaitFor<ICondition>()
            .Http()
            .Url(FakeApiRequestUrl));
    }
}
