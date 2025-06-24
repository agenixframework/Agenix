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
using Agenix.Api.Spi;
using Agenix.GraphQL.Actions;
using ITestAction = Agenix.Api.ITestAction;


namespace Agenix.GraphQL.Tests.Actions;

public class GraphQLActionBuilderTest
{
    private GraphQLActionBuilder? _fixture;

    [SetUp]
    public void SetUp()
    {
        _fixture = new GraphQLActionBuilder();
    }

    [Test]
    public void ShouldLookupTestActionBuilder()
    {
        var endpointBuilders = ITestActionBuilder<ITestAction>.Lookup();
        Assert.That(endpointBuilders.ContainsKey("graphql"), Is.True);

        var graphqlBuilder = ITestActionBuilder<ITestAction>.Lookup("graphql");
        Assert.That(graphqlBuilder.IsPresent, Is.True);
        Assert.That(graphqlBuilder.Value.GetType(), Is.EqualTo(typeof(GraphQLActionBuilder)));
    }

    [Test]
    public void IsReferenceResolverAwareTestActionBuilder()
    {
        Assert.That(_fixture, Is.InstanceOf<AbstractReferenceResolverAwareTestActionBuilder<ITestAction>>(),
            "Is instanceof AbstractReferenceResolverAwareTestActionBuilder");
    }
}
