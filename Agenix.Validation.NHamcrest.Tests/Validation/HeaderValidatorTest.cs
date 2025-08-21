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
using Agenix.Core.Validation;
using Agenix.Validation.NHamcrest.Validation;

namespace Agenix.Validation.NHamcrest.Tests.Validation;

/// <summary>
///     Unit tests for the <see cref="IHeaderValidator" /> interface and its default implementation
///     <see cref="DefaultHeaderValidator" />.
/// </summary>
public class HeaderValidatorTest
{
    [Test]
    public void TestLookup()
    {
        var validators = IHeaderValidator.Lookup();
        Assert.That(validators.Count, Is.EqualTo(2));
        Assert.That(validators["defaultHeaderValidator"], Is.Not.Null);
        using (Assert.EnterMultipleScope())
        {
            Assert.That(validators["defaultHeaderValidator"].GetType(), Is.EqualTo(typeof(DefaultHeaderValidator)));
            Assert.That(validators["hamcrestHeaderValidator"], Is.Not.Null);
        }

        Assert.That(validators["hamcrestHeaderValidator"].GetType(), Is.EqualTo(typeof(NHamcrestHeaderValidator)));
    }

    [Test]
    public void TestDefaultLookup()
    {
        var validator = IHeaderValidator.Lookup("default");
        Assert.That(validator.IsPresent, Is.True);
        validator = IHeaderValidator.Lookup("nhamcrest");
        Assert.That(validator.IsPresent, Is.True);
    }
}
