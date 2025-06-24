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
using Agenix.Api.Exceptions;
using Agenix.Core.Validation.Matcher.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Matcher.Core;

public class GreaterThanValidationMatcherTest : AbstractNUnitSetUp
{
    private readonly GreaterThanValidationMatcher _matcher = new();

    [Test]
    public void TestValidateSuccess()
    {
        _matcher.Validate("field", "3", new List<string> { "2" }, Context);
        _matcher.Validate("field", "1", new List<string> { "-1" }, Context);
        _matcher.Validate("field", "0.000000001", new List<string> { "0" }, Context);
        _matcher.Validate("field", "0", new List<string> { "-0.000000001" }, Context);
    }

    [Test]
    public void TestValidateError()
    {
        AssertException("field", "NaN", new List<string> { "2" });
        AssertException("field", "2", new List<string> { "NaN" });
        AssertException("field", "2.0", new List<string> { "2.0" });
        AssertException("field", "2.0", new List<string> { "2.1" });
    }

    private void AssertException(string fieldName, string value, List<string> control)
    {
        try
        {
            _matcher.Validate(fieldName, value, control, Context);
            Assert.Fail("Expected exception not thrown!");
        }
        catch (ValidationException e)
        {
            ClassicAssert.IsTrue(e.GetMessage().Contains(fieldName));
            ClassicAssert.IsTrue(e.GetMessage().Contains(control[0]));
            ClassicAssert.IsTrue(e.GetMessage().Contains(value));
        }
    }
}
