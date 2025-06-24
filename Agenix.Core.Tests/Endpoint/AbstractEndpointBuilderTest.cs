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
using Agenix.Api.Annotations;
using Agenix.Api.Endpoint;
using Agenix.Core.Endpoint;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Endpoint;

public class AbstractEndpointBuilderTest : AbstractNUnitSetUp
{
    private readonly TestEndpointBuilder _endpointBuilder = new();
    private readonly TestEndpointBuilder.PersonClass _person = new("Peter", 29);
    [AgenixEndpoint("fooEndpoint",
        "message:Hello from Agenix!",
        "number:1:System.Int32",
        "person:testPerson:Agenix.Core.Tests.Endpoint.AbstractEndpointBuilderTest+TestEndpointBuilder+PersonClass"
    )]
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private IEndpoint _injected;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    protected override TestContextFactory CreateTestContextFactory()
    {
        var contextFactory = base.CreateTestContextFactory();
        contextFactory.ReferenceResolver.Bind("testBuilder", _endpointBuilder);
        contextFactory.ReferenceResolver.Bind("testPerson", _person);
        return contextFactory;
    }

    [Test]
    public void BuildFromEndpointProperties()
    {
        AgenixEndpointAnnotations.InjectEndpoints(this, Context);

        Console.WriteLine(typeof(TestEndpointBuilder.PersonClass).FullName);

        ClassicAssert.AreEqual(_injected, _endpointBuilder.MockEndpoint);
        ClassicAssert.AreEqual(_endpointBuilder.Message, "Hello from Agenix!");
        ClassicAssert.AreEqual(_endpointBuilder.Number, 1);
        //ClassicAssert.AreEqual(_endpointBuilder.Person, _person);
    }

    public class TestEndpointBuilder : AbstractEndpointBuilder<IEndpoint>
    {
        public string Message { get; private set; }
        public PersonClass Person { get; private set; }
        public int Number { get; private set; }

        public IEndpoint MockEndpoint { get; } = new Mock<IEndpoint>().Object;

        protected override IEndpoint GetEndpoint()
        {
            return MockEndpoint;
        }

        public override bool Supports(Type endpointType)
        {
            return true;
        }

        public TestEndpointBuilder SetMessage(string message)
        {
            Message = message;
            return this;
        }

        public TestEndpointBuilder SetNumber(int number)
        {
            Number = number;
            return this;
        }

        public TestEndpointBuilder SetPerson(PersonClass person)
        {
            Person = person;
            return this;
        }

        public class PersonClass(string name, int age)
        {
            public string Name { get; } = name;
            public int Age { get; } = age;
        }
    }
}
