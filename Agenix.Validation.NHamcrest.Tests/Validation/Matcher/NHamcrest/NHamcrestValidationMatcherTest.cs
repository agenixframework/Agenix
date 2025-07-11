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

using Agenix.Validation.NHamcrest.Validation.Matcher;

namespace Agenix.Validation.NHamcrest.Tests.Validation.Matcher.NHamcrest;

public class NHamcrestValidationMatcherTest : AbstractNUnitSetUp
{
    private readonly NHamcrestValidationMatcher _validationMatcher = new();

    public static IEnumerable<TestCaseData> SuccessData
    {
        get
        {
            yield return new TestCaseData("foo", "value", new List<string> { "EqualTo(value)" });
            yield return new TestCaseData("foo", "value", new List<string> { "EqualTo('value')" });
            yield return new TestCaseData("foo", "value with ' quote",
                new List<string> { "EqualTo('value with \\' quote')" });
            yield return new TestCaseData("foo", "value with ' quote",
                new List<string> { "EqualTo(value with \\' quote)" });
            yield return new TestCaseData("foo", "value", new List<string> { "Not(EqualTo(other))" });
            yield return new TestCaseData("foo", "value", new List<string> { "EqualToIgnoringCase(VALUE)" });
            yield return new TestCaseData("foo", "value with ' quote",
                new List<string> { "EqualToIgnoringCase(VALUE WITH \\' QUOTE)" });
            yield return new TestCaseData("foo", "value", new List<string> { "Not(EqualToIgnoringCase(OTHER))" });
            yield return new TestCaseData("foo", "no extra spaces",
                new List<string> { "EqualToIgnoringWhiteSpace(no extra spaces  )" });
            yield return new TestCaseData("foo", "with   spaces",
                new List<string> { "EqualToIgnoringWhiteSpace(with spaces)" });
            yield return new TestCaseData("foo", "string with   extra    spaces",
                new List<string> { "EqualToIgnoringWhiteSpace(string   with extra spaces)" });
            yield return new TestCaseData("foo", "value", new List<string> { "ContainsString('lue')" });
            yield return new TestCaseData("foo", "value with ' quote", new List<string> { "ContainsString(with \\')" });
            yield return new TestCaseData("foo", "value with ' quote", new List<string> { "ContainsString(\\')" });
            yield return new TestCaseData("foo", "value with ' quote",
                new List<string> { "ContainsString(value with \\' qu)" });
            yield return new TestCaseData("foo", "value", new List<string> { "Not(ContainsString('other'))" });
            yield return new TestCaseData("foo", "value", new List<string> { "ContainsStringIgnoringCase('LUE')" });
            yield return new TestCaseData("foo", "value with ' quote",
                new List<string> { "ContainsStringIgnoringCase(WITH \\')" });
            yield return new TestCaseData("foo", "value with ' quote",
                new List<string> { "ContainsStringIgnoringCase(\\')" });
            yield return new TestCaseData("foo", "value with ' quote",
                new List<string> { "ContainsStringIgnoringCase(VALUE with \\' QU)" });
            yield return new TestCaseData("foo", "value",
                new List<string> { "Not(ContainsStringIgnoringCase('OTHER'))" });
            yield return new TestCaseData("foo", "value", new List<string> { "StartsWith('v')" });
            yield return new TestCaseData("foo", "value", new List<string> { "Not(StartsWith('o'))" });
            yield return new TestCaseData("foo", "value", new List<string> { "StartsWithIgnoringCase('V')" });
            yield return new TestCaseData("foo", "value", new List<string> { "Not(StartsWithIgnoringCase('O'))" });
            yield return new TestCaseData("foo", "value", new List<string> { "EndsWith('e')" });
            yield return new TestCaseData("foo", "value", new List<string> { "Not(EndsWith('o'))" });
            yield return new TestCaseData("foo", "value", new List<string> { "EndsWithIgnoringCase('E')" });
            yield return new TestCaseData("foo", "value", new List<string> { "Not(EndsWithIgnoringCase('O'))" });
            yield return new TestCaseData("foo", "value", new List<string> { "MatchesPattern('.*')" });
            yield return new TestCaseData("foo", "value", new List<string> { "Not(MatchesPattern('other'))" });
            yield return new TestCaseData("foo", "value", new List<string> { "AnyOf(StartsWith(val), EndsWith(lue))" });
            yield return new TestCaseData("foo", "value", new List<string> { "AllOf(StartsWith(val), EndsWith(lue))" });
            yield return new TestCaseData("foo", "value", new List<string> { "IsOneOf(value, other)" });
            yield return new TestCaseData("foo", "1", new List<string> { "IsOneOf(1, 2)" });
            yield return new TestCaseData("foo", "value with ' quote",
                new List<string> { "IsOneOf('value with \\' quote', 'other')" });
            yield return new TestCaseData("foo", "test value", new List<string> { "IsOneOf('test value', 'other ')" });
            yield return new TestCaseData("foo", "9.0", new List<string> { "IsOneOf(9, 9.0)" });
            yield return new TestCaseData("foo", "value", new List<string> { "IsIn(value, other)" });
            yield return new TestCaseData("foo", "1", new List<string> { "IsIn(1, 2)" });
            yield return new TestCaseData("foo", "value with ' quote",
                new List<string> { "IsIn('value with \\' quote', 'other')" });
            yield return new TestCaseData("foo", "test value", new List<string> { "IsIn('test value', 'other ')" });
            yield return new TestCaseData("foo", "9.0", new List<string> { "IsIn(9, 9.0)" });
            yield return new TestCaseData("foo", "value1", new List<string> { "MatchesPattern([^2345]*)" });
            yield return new TestCaseData("foo", "value1 with quotes",
                new List<string> { "MatchesPattern('[^2345]*')" });
            yield return new TestCaseData("foo", null, new List<string> { "NullValue()" });
            yield return new TestCaseData("foo", "bar", new List<string> { "NotNullValue()" });
            yield return new TestCaseData("foo", "", new List<string> { "IsEmptyString()" });
            yield return new TestCaseData("foo", "bar", new List<string> { "Not(IsEmptyString())" });
            yield return new TestCaseData("foo", null, new List<string> { "IsEmptyOrNullString()" });
            yield return new TestCaseData("foo", " ", new List<string> { "BlankString()" });
            yield return new TestCaseData("foo", null, new List<string> { "BlankOrNullString()" });
            yield return new TestCaseData("foo", "[]", new List<string> { "Empty()" });
            yield return new TestCaseData("foo", "", new List<string> { "Empty()" });
            //yield return new TestCaseData("foo", "10", new List<string> { "GreaterThan(9)" });
        }
    }

    [TestCaseSource(nameof(SuccessData))]
    public void TestValidateSuccess(string path, string value, List<string> parameters)
    {
        _validationMatcher.Validate("foo", value, parameters, Context);
    }
}
