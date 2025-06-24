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
using Agenix.Validation.Json.Validation;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Validation;

/// <summary>
///     Unit tests for the <see cref="MessageValidator" /> class.
/// </summary>
public class MessageValidatorTest
{
    [Test]
    public void TestLookup()
    {
        var validators = IMessageValidator<IValidationContext>.Lookup();
        ClassicAssert.AreEqual(3, validators.Count);
        ClassicAssert.IsNotNull(validators["header"]);
        ClassicAssert.AreEqual(validators["header"].GetType(), typeof(DefaultMessageHeaderValidator));
        ClassicAssert.IsNotNull(validators["json"]);
        ClassicAssert.AreEqual(validators["json"].GetType(),
            typeof(JsonTextMessageValidator));
        ClassicAssert.IsNotNull(validators["json-path"]);
        ClassicAssert.AreEqual(validators["json-path"].GetType(),
            typeof(JsonPathMessageValidator));
    }

    [Test]
    public void TestTestLookup()
    {
        ClassicAssert.IsTrue(IMessageValidator<IValidationContext>.Lookup("header").IsPresent);
    }
}
