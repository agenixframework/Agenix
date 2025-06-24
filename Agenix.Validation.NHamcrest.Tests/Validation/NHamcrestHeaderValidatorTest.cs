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
using Agenix.Api.Validation.Context;
using Agenix.Validation.NHamcrest.Validation;
using Is = NHamcrest.Is;

namespace Agenix.Validation.NHamcrest.Tests.Validation;

public class NHamcrestHeaderValidatorTest : AbstractNUnitSetUp
{
    private readonly HeaderValidationContext _validationContext = new();
    private readonly NHamcrestHeaderValidator _validator = new();

    public static IEnumerable<TestCaseData> SuccessData
    {
        get
        {
            yield return new TestCaseData("foo", "foo");
            yield return new TestCaseData("foo", Is.EqualTo("foo"));
        }
    }

    public static IEnumerable<TestCaseData> ErrorData
    {
        get
        {
            yield return new TestCaseData("foo", "wrong");
            yield return new TestCaseData("foo", Is.EqualTo("wrong"));
        }
    }

    [TestCaseSource(nameof(SuccessData))]
    public void TestValidateHeaderSuccess(object receivedValue, object controlValue)
    {
        _validator.ValidateHeader("foo", receivedValue, controlValue, Context, _validationContext);
    }

    [TestCaseSource(nameof(ErrorData))]
    public void TestValidateHeaderError(object receivedValue, object controlValue)
    {
        Assert.Throws<ValidationException>(() =>
        {
            _validator.ValidateHeader("foo", receivedValue, controlValue, Context, _validationContext);
        });
    }
}
