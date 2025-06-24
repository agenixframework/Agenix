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

using Agenix.Validation.Json.Validation;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Validation.Json.Tests.Json;

public class JsonElementValidatorItemTest
{
    public static IEnumerable<TestCaseData> GetPathPairs()
    {
        yield return new TestCaseData(
            "$['propertyA']",
            new JsonElementValidatorItem<string>("propertyA", "", "")
        );

        yield return new TestCaseData(
            "$['propertyA']",
            new JsonElementValidatorItem<string>("propertyA", "", "")
                .Parent(new JsonElementValidatorItem<string>(null, "", ""))
        );

        yield return new TestCaseData(
            "$['propertyA']['propertyB']",
            new JsonElementValidatorItem<string>("propertyB", "", "")
                .Parent(new JsonElementValidatorItem<string>("propertyA", "", "")
                    .Parent(new JsonElementValidatorItem<string>(null, "", "")))
        );

        yield return new TestCaseData(
            "$['propertyA'][1]",
            new JsonElementValidatorItem<string>(1, "", "")
                .Parent(new JsonElementValidatorItem<string>("propertyA", "", "")
                    .Parent(new JsonElementValidatorItem<string>(null, "", "")))
        );

        yield return new TestCaseData(
            "$[1]",
            new JsonElementValidatorItem<string>(1, "", "")
                .Parent(new JsonElementValidatorItem<string>(null, "", ""))
        );
    }

    [Test]
    [TestCaseSource(nameof(GetPathPairs))]
    public void ShouldGetJsonPath(string expectedPath, JsonElementValidatorItem<string> fixture)
    {
        Assert.That(fixture.GetJsonPath(), Is.EqualTo(expectedPath));
    }

    public static IEnumerable<TestCaseData> GetNamePairs()
    {
        yield return new TestCaseData(
            "$",
            new JsonElementValidatorItem<string>(null, "", "")
        );

        yield return new TestCaseData(
            "propertyA",
            new JsonElementValidatorItem<string>("propertyA", "", "")
        );

        yield return new TestCaseData(
            "[2]",
            new JsonElementValidatorItem<string>(2, "", "")
        );
    }

    [Test]
    [TestCaseSource(nameof(GetNamePairs))]
    public void ShouldGetName(string expectedName, JsonElementValidatorItem<string> fixture)
    {
        Assert.That(fixture.GetName(), Is.EqualTo(expectedName));
    }
}
