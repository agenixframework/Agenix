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

using System.Collections.Generic;
using Agenix.Api.Validation.Matcher;
using Moq;
using NUnit.Framework;

namespace Agenix.Core.Tests.Matcher;

public class ValidationMatcherUtilsTest : AbstractNUnitSetUp
{
    private readonly Mock<IValidationMatcher> _matcher = new();
    private ValidationMatcherLibrary _validationMatcherLibrary = new();

    [OneTimeSetUp]
    public void TestFixtureSetUp()
    {
        _validationMatcherLibrary = new ValidationMatcherLibrary
        {
            Name = "fooValidationMatcherLibrary",
            Prefix = "foo:"
        };
        _validationMatcherLibrary.Members.Add("CustomMatcher", _matcher.Object);
    }

    [Test]
    public void TestResolveDefaultValidationMatcher()
    {
        ValidationMatcherUtils.ResolveValidationMatcher("field", "value", "@Ignore@", Context);
        ValidationMatcherUtils.ResolveValidationMatcher("field", "value", "@Ignore()@", Context);
        ValidationMatcherUtils.ResolveValidationMatcher("field", "value", "@Ignore('bad syntax')@", Context);
        ValidationMatcherUtils.ResolveValidationMatcher("field", "value", "@EqualsIgnoreCase('VAlUe')@", Context);
        ValidationMatcherUtils.ResolveValidationMatcher("field", "value", "@${EqualsIgnoreCase('value')}@",
            Context);
        ValidationMatcherUtils.ResolveValidationMatcher("field", "value", "@${EqualsIgnoreCase(value)}@", Context);
        ValidationMatcherUtils.ResolveValidationMatcher("field", "John's", "@EqualsIgnoreCase('John's')@", Context);
        ValidationMatcherUtils.ResolveValidationMatcher("field", "John's&Barabara's",
            "@EqualsIgnoreCase('John's&Barabara's')@", Context);
        ValidationMatcherUtils.ResolveValidationMatcher("field", "", "@EqualsIgnoreCase('')@", Context);
        ValidationMatcherUtils.ResolveValidationMatcher("field", "prefix:value",
            "@EqualsIgnoreCase('prefix:value')@", Context);
    }

    [Test]
    public void TestResolveCustomValidationMatcher()
    {
        _matcher.Reset();

        Context.ValidationMatcherRegistry.AddValidationMatcherLibrary(_validationMatcherLibrary);
        ValidationMatcherUtils.ResolveValidationMatcher("field", "value", "@foo:CustomMatcher('value')@", Context);
        ValidationMatcherUtils.ResolveValidationMatcher("field", "value", "@foo:CustomMatcher(value)@", Context);
        ValidationMatcherUtils.ResolveValidationMatcher("field", "value", "@${foo:CustomMatcher('value')}@",
            Context);
        ValidationMatcherUtils.ResolveValidationMatcher("field", "prefix:value",
            "@foo:CustomMatcher('prefix:value')@", Context);

        _matcher.Verify(s => s.Validate("field", "value", new List<string> { "value" }, Context), Times.Exactly(3));
        _matcher.Verify(s => s.Validate("field", "value", new List<string> { "value" }, Context));
    }
}
