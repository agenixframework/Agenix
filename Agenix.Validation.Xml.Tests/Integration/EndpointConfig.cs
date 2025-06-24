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

using Agenix.Api.Message;
using Agenix.Api.Spi;
using Agenix.Api.Xml.Namespace;
using Agenix.Core.Endpoint.Direct;
using Agenix.Core.Message;

namespace Agenix.Validation.Xml.Tests.Integration;

/// Represents the configuration for endpoints used in the application.
/// The class provides methods to configure and bind various objects necessary for
/// routing messages and managing namespaces as part of the messaging system.
public class EndpointConfig
{
    [BindToRegistry(Name = "hello.queue")]
    public IMessageQueue HelloQueue()
    {
        return new DefaultMessageQueue("helloQueue");
    }

    [BindToRegistry(Name = "hello.endpoint")]
    public DirectEndpoint HelloEndpoint()
    {
        return new DirectEndpointBuilder()
            .Queue(HelloQueue())
            .Build();
    }

    [BindToRegistry(Name = "namespaceContextBuilder")]
    public NamespaceContextBuilder NamespaceContextBuilder()
    {
        var builder = new NamespaceContextBuilder();
        builder.NamespaceMappings.Add("def", "http://agenix.org/schemas/samples/HelloService.xsd");
        return builder;
    }
}
