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

using Agenix.Api.Exceptions;
using Agenix.Api.Variable;
using Agenix.Validation.Json.Json;
using NUnit.Framework;

namespace Agenix.Validation.Json.Tests.Json;

[TestFixture]
public class JsonPathSegmentVariableExtractorTests : AbstractNUnitSetUp
{
    private const string JsonFixture = "{\"name\": \"Peter\"}";

    private readonly JsonPathSegmentVariableExtractor _unitUnderTest = new();

    [Test]
    public void TestExtractFromJson()
    {
        var jsonPath = "$.name";
        var matcher = MatchSegmentExpressionMatcher(jsonPath);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_unitUnderTest.CanExtract(Context, JsonFixture, matcher), Is.True);
            Assert.That(_unitUnderTest.ExtractValue(Context, JsonFixture, matcher), Is.EqualTo("Peter"));
        }
    }

    [Test]
    public void TestExtractFromNonJsonPathExpression()
    {
        var json = "{\"name\": \"Peter\"}";

        var nonJsonPath = "name";
        var matcher = MatchSegmentExpressionMatcher(nonJsonPath);

        Assert.That(_unitUnderTest.CanExtract(Context, json, matcher), Is.False);
    }

    [Test]
    public void TestExtractFromJsonExpressionFailure()
    {
        var json = "{\"name\": \"Peter\"}";

        var invalidJsonPath = "$.$$$name";
        var matcher = MatchSegmentExpressionMatcher(invalidJsonPath);

        Assert.That(_unitUnderTest.CanExtract(Context, json, matcher), Is.True);

        // NUnit way to assert exceptions
        Assert.Throws<AgenixSystemException>(() =>
            _unitUnderTest.ExtractValue(Context, json, matcher));
    }

    /// <summary>
    ///     Create a variable expression jsonPath matcher and match the first jsonPath
    /// </summary>
    /// <param name="jsonPath">The JSON path to match</param>
    /// <returns>A matcher that has found its first match</returns>
    private VariableExpressionSegmentMatcher MatchSegmentExpressionMatcher(string jsonPath)
    {
        var variableExpression = $"jsonPath({jsonPath})";
        var matcher = new VariableExpressionSegmentMatcher(variableExpression);
        Assert.That(matcher.NextMatch(), Is.True);
        return matcher;
    }
}
