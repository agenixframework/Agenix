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
using Agenix.Api.Message;
using Agenix.Http.Actions;
using Agenix.Http.Message;
using static System.Net.HttpStatusCode;
using static Moq.Mock;

namespace Agenix.Http.Tests.Actions;

public class HttpClientResponseActionBuilderTest
{
    private HttpMessage _httpMessageMock;
    private IMessageBuilder _messageBuilderMock;

    [SetUp]
    public void BeforeMethodSetup()
    {
        _httpMessageMock = Of<HttpMessage>();
        _messageBuilderMock = Of<IMessageBuilder>();
    }

    [Test]
    public void StatusFromHttpStatus()
    {
        new HttpClientResponseActionBuilder(_messageBuilderMock, _httpMessageMock)
            .Message()
            .Status(OK); // Method under test

        Get(_httpMessageMock).Verify(m => m.Status(OK));
    }

    [Test]
    public void StatusFromHttpStatusCode()
    {
        var httpStatusCode = (HttpStatusCode)123;

        new HttpClientResponseActionBuilder(_messageBuilderMock, _httpMessageMock)
            .Message()
            .Status(httpStatusCode); // Method under test

        Get(_httpMessageMock).Verify(m => m.Status(httpStatusCode));
    }
}
