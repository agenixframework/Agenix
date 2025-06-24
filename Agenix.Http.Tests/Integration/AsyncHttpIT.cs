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

using System.Net;
using System.Net.Mime;
using Agenix.Api.Annotations;
using Agenix.Api.Message;
using Agenix.Core;
using Agenix.Http.Actions;
using Agenix.Http.Client;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using static Agenix.Core.Container.Async.Builder;
using static Agenix.Core.Variable.MessageHeaderVariableExtractor.Builder;
using HttpClient = Agenix.Http.Client.HttpClient;

namespace Agenix.Http.Tests.Integration;

[NUnitAgenixSupport]
public class AsyncHttpIT
{
    private const string fakeApiRequestUrl = "https://jsonplaceholder.typicode.com";

    private readonly HttpClient _client = new HttpClientBuilder()
        .RequestUrl(fakeApiRequestUrl)
        .Build();

    [AgenixResource]
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private IGherkinTestActionRunner _gherkin;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [Test]
    public void AsyncHttpPost()
    {
        _gherkin.When(Async().Actions(
                HttpActionBuilder.Http()
                    .Client(_client)
                    .Send()
                    .Post("/posts")
                    .Message()
                    .Body("{\"title\":\"foo\",\"body\":\"bar\",\"userId\":1}")
                    .ContentType(MediaTypeNames.Application.Json)
                    .Extract(FromHeaders().Header(MessageHeaders.Id, "request#1")),
                HttpActionBuilder.Http()
                    .Client(_client)
                    .Receive()
                    .Response(HttpStatusCode.Created)
                    .Message()
                    .Type(MessageType.JSON)
                    .Body("{\"title\":\"foo\",\"body\":\"bar\",\"userId\":1,\"id\": 101}")
                    .Selector(new Dictionary<string, object> { [MessageHeaders.Id] = "${request#1}" })
            )
        );

        _gherkin.When(Async().Actions(
                HttpActionBuilder.Http()
                    .Client(_client)
                    .Send()
                    .Post("/posts")
                    .Message()
                    .Body("{\"title\":\"foo1\",\"body\":\"bar1\",\"userId\":2}")
                    .ContentType(MediaTypeNames.Application.Json)
                    .Extract(FromHeaders().Header(MessageHeaders.Id, "request#2")),
                HttpActionBuilder.Http()
                    .Client(_client)
                    .Receive()
                    .Response(HttpStatusCode.Created)
                    .Message()
                    .Type(MessageType.JSON)
                    .Body("{\"title\":\"foo1\",\"body\":\"bar1\",\"userId\":2,\"id\": 101}")
                    .Selector(new Dictionary<string, object> { [MessageHeaders.Id] = "${request#2}" })
            )
        );
    }
}
