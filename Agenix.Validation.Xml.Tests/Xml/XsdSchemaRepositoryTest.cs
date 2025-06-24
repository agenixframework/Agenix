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

using Agenix.Validation.Xml.Schema;
using NUnit.Framework;

namespace Agenix.Validation.Xml.Tests.Xml;

public class XsdSchemaRepositoryTest
{
    [Test]
    public void TestUnknownLocation()
    {
        var schemaRepository = new XsdSchemaRepository();

        schemaRepository.Locations.Add("file:org/agenix/unknown/unknown.xsd");

        schemaRepository.Initialize();
        Assert.That(schemaRepository.GetSchemas().Count, Is.EqualTo(0));
    }

    [Test]
    public void TestWsdlResourceLocation()
    {
        var schemaRepository = new XsdSchemaRepository();

        schemaRepository.Locations.Add(
            "assembly://Agenix.Validation.Xml.Tests/Agenix.Validation.Xml.Tests.Resources.Validation/TestService.wsdl");

        //schemaRepository.Initialize();

        //Assert.That(schemaRepository.GetSchemas().Count, Is.EqualTo(1));
        //Assert.That(schemaRepository.GetSchemas()[0], Is.InstanceOf<WsdlXsdSchema>());
    }

    [Test]
    public void TestXsdResourceLocation()
    {
        var schemaRepository = new XsdSchemaRepository();

        schemaRepository.Locations.Add(
            "assembly://Agenix.Validation.Xml.Tests/Agenix.Validation.Xml.Tests.Resources.Validation/test.xsd");

        schemaRepository.Initialize();

        Assert.That(schemaRepository.GetSchemas().Count, Is.EqualTo(1));
        Assert.That(schemaRepository.GetSchemas()[0], Is.InstanceOf<SimpleXsdSchema>());
    }
}
