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
using Agenix.Api.Validation.Context;
using Agenix.Core.Validation;
using NUnit.Framework;

namespace Agenix.Core.Tests.Validation;

/// <summary>
///     Contains unit tests for the DefaultHeaderValidator class.
/// </summary>
public class DefaultHeaderValidatorTest : AbstractNUnitSetUp
{
    private readonly HeaderValidationContext _validationContext = new();
    private readonly DefaultHeaderValidator _validator = new();

    [Test]
    public void TestValidateHeader()
    {
        _validator.ValidateHeader("foo", "foo", "foo", Context, _validationContext);
        _validator.ValidateHeader("foo", null, "", Context, _validationContext);
        _validator.ValidateHeader("foo", null, null, Context, _validationContext);
        _validator.ValidateHeader("foo", new List<string> { "foo", "bar" }, new List<string> { "foo", "bar" }, Context,
            _validationContext);
        _validator.ValidateHeader("foo", new[] { "foo", "bar" }, new[] { "foo", "bar" }, Context,
            _validationContext);
        _validator.ValidateHeader("foo", new Dictionary<object, object> { { "foo", "bar" } },
            new Dictionary<object, object> { { "foo", "bar" } }, Context,
            _validationContext);
    }

    [Test]
    public void TestValidateHeaderVariableSupport()
    {
        Context.SetVariable("control", "bar");

        _validator.ValidateHeader("foo", "bar", "${control}", Context, _validationContext);
    }

    [Test]
    public void TestValidateHeaderValidationMatcherSupport()
    {
        _validator.ValidateHeader("foo", "bar", "@Ignore@", Context, _validationContext);
        _validator.ValidateHeader("foo", "bar", "@StringLength(3)@", Context, _validationContext);
    }

    [Test]
    public void TestValidateHeaderError()
    {
        Assert.Throws<ValidationException>(() =>
            _validator.ValidateHeader("foo", "foo", "wrong", Context, _validationContext));
        Assert.Throws<ValidationException>(() =>
            _validator.ValidateHeader("foo", null, "wrong", Context, _validationContext));
        Assert.Throws<ValidationException>(() =>
            _validator.ValidateHeader("foo", "foo", null, Context, _validationContext));
        Assert.Throws<ValidationException>(() =>
            _validator.ValidateHeader("foo", new List<string> { "foo", "bar" }, new List<string> { "foo", "wrong" },
                Context, _validationContext));
        Assert.Throws<ValidationException>(() =>
            _validator.ValidateHeader("foo", new[] { "foo", "bar" }, new[] { "foo", "wrong" },
                Context, _validationContext));
        Assert.Throws<ValidationException>(() =>
            _validator.ValidateHeader("foo", new Dictionary<object, object> { { "foo", "bar" } },
                new Dictionary<object, object> { { "foo", "wrong" } },
                Context, _validationContext));
    }
}
