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

using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Agenix.Core.Validation;
using Agenix.Validation.Xml.Validation.Xhtml;
using Agenix.Validation.Xml.Validation.Xml;
using NUnit.Framework;

namespace Agenix.Validation.Xml.Tests.Validation;

public class MessageValidatorTest
{
    [Test]
    public void TestLookup()
    {
        var validators = IMessageValidator<IValidationContext>.Lookup();
        Assert.That(validators.Count, Is.EqualTo(5));
        Assert.That(validators.ContainsKey("header"), Is.True);
        Assert.That(validators["header"].GetType(), Is.EqualTo(typeof(DefaultMessageHeaderValidator)));
        Assert.That(validators.ContainsKey("xml"), Is.True);
        Assert.That(validators["xml"].GetType(), Is.EqualTo(typeof(DomXmlMessageValidator)));
        Assert.That(validators.ContainsKey("xpath"), Is.True);
        Assert.That(validators["xpath"].GetType(), Is.EqualTo(typeof(XpathMessageValidator)));
        Assert.That(validators.ContainsKey("xhtml"), Is.True);
        Assert.That(validators["xhtml"].GetType(), Is.EqualTo(typeof(XhtmlMessageValidator)));
        Assert.That(validators.ContainsKey("xhtml-xpath"), Is.True);
        Assert.That(validators["xhtml-xpath"].GetType(), Is.EqualTo(typeof(XhtmlXpathMessageValidator)));
    }

    [Test]
    public void TestTestLookup()
    {
        Assert.That(IMessageValidator<IValidationContext>.Lookup("header").IsPresent, Is.True);
        Assert.That(IMessageValidator<IValidationContext>.Lookup("xml").IsPresent, Is.True);
        Assert.That(IMessageValidator<IValidationContext>.Lookup("xpath").IsPresent, Is.True);
        Assert.That(IMessageValidator<IValidationContext>.Lookup("xhtml").IsPresent, Is.True);
        Assert.That(IMessageValidator<IValidationContext>.Lookup("xhtml-xpath").IsPresent, Is.True);
    }
}
