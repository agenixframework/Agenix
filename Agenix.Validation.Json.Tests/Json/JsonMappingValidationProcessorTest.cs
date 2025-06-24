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
using Agenix.Core.Message;
using Agenix.Core.Validation.Json;
using Newtonsoft.Json;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Validation.Json.Tests.Json;

public class JsonMappingValidationProcessorTest : AbstractNUnitSetUp
{
    private readonly JsonSerializer _jsonSerializer = new();
    private readonly IMessage _message = new DefaultMessage("{\"name\": \"John\", \"age\": 23}");

    [Test]
    public void ShouldValidate()
    {
        var processor = JsonMappingValidationProcessor<Person>
            .Validate()
            .JsonSerializer(_jsonSerializer)
            .Validator((person, headers, context) =>
            {
                Assert.That(person.Name, Is.EqualTo("John"));
                Assert.That(person.Age, Is.EqualTo(23));
            })
            .Build();

        processor.Validate(_message, Context);
    }

    [Test]
    public void ShouldResolveMapper()
    {
        var referenceResolver = new SimpleReferenceResolver();
        referenceResolver.Bind("serializer", _jsonSerializer);

        var processor = JsonMappingValidationProcessor<Person>
            .Validate()
            .WithReferenceResolver(referenceResolver)
            .Validator((person, headers, context) =>
            {
                Assert.That(person.Name, Is.EqualTo("John"));
                Assert.That(person.Age, Is.EqualTo(23));
            })
            .Build();

        processor.Validate(_message, Context);
    }

    [Test]
    public void ShouldFailValidation()
    {
        var processor = JsonMappingValidationProcessor<Person>
            .Validate()
            .JsonSerializer(_jsonSerializer)
            .Validator((person, headers, context) => { Assert.That(person.Age, Is.EqualTo(-1)); })
            .Build();

        Assert.Throws<AssertionException>(() => processor.Validate(_message, Context));
    }

    /// <summary>
    ///     Represents a person with a name and an age.
    /// </summary>
    private sealed record Person
    {
        // Auto-implemented properties in C#
        public int Age { get; set; }
        public string Name { get; set; }
    }
}
