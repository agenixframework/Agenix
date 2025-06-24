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
using System.Collections.Generic;
using Agenix.Api.Exceptions;
using Agenix.Api.Validation.Matcher;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Matcher;

public class DefaultControlExpressionParserTest
{
    [Test]
    [TestCaseSource(nameof(ValidControlExpressions))]
    public void ShouldExtractControlParametersSuccessfully(string controlExpression, List<string> expectedParameters)
    {
        IControlExpressionParser expressionParser = new DefaultControlExpressionParser();
        var extractedParameters = expressionParser.ExtractControlValues(controlExpression, '\0');

        Console.WriteLine("Expected Parameters: " + string.Join(", ", expectedParameters));
        Console.WriteLine("Extracted Parameters: " + string.Join(", ", extractedParameters));

        ClassicAssert.AreEqual(extractedParameters.Count, expectedParameters.Count);

        for (var i = 0; i < expectedParameters.Count; i++)
        {
            ClassicAssert.True(extractedParameters.Count > i);
            ClassicAssert.AreEqual(extractedParameters[i], expectedParameters[i]);
        }
    }

    [Test]
    [TestCaseSource(nameof(InvalidControlExpressions))]
    public void ShouldNotExtractControlParametersSuccessfully(string controlExpression)
    {
        IControlExpressionParser expressionParser = new DefaultControlExpressionParser();

        Assert.Throws<AgenixSystemException>(() => expressionParser.ExtractControlValues(controlExpression, '\0'));
    }

    public static IEnumerable<TestCaseData> ValidControlExpressions()
    {
        yield return new TestCaseData("'a'", new List<string> { "a" });
        yield return new TestCaseData("'a',", new List<string> { "a" });
        yield return new TestCaseData("'a','b'", new List<string> { "a", "b" });
        yield return new TestCaseData("'a','b',", new List<string> { "a", "b" });
        yield return new TestCaseData("'a,s','b',", new List<string> { "a,s", "b" });
        yield return new TestCaseData("'a)s','b',", new List<string> { "a)s", "b" });
        yield return new TestCaseData("'a's','b',", new List<string> { "a's", "b" });
        yield return new TestCaseData("''", new List<string> { "" });
        yield return new TestCaseData("'',", new List<string> { "" });
        yield return new TestCaseData("", new List<string>());
        yield return new TestCaseData(null, new List<string>());
    }

    public static IEnumerable<TestCaseData> InvalidControlExpressions()
    {
        yield return new TestCaseData("'");
        yield return new TestCaseData("',");
        yield return new TestCaseData("'a");
        yield return new TestCaseData("'a,");
        yield return new TestCaseData("'a's,");
        yield return new TestCaseData("'a',s'");
        yield return new TestCaseData("'a','b");
        yield return new TestCaseData("'a','b,");
    }
}
