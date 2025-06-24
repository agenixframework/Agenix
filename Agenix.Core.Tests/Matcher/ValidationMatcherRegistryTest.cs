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
using Agenix.Api.Validation.Matcher;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Matcher;

public class ValidationMatcherRegistryTest : AbstractNUnitSetUp
{
    private readonly Mock<IValidationMatcher> _matcher = new();
    private ValidationMatcherLibrary _validationMatcherLibrary;

    [OneTimeSetUp]
    public void TestFixtureSetUp()
    {
        _validationMatcherLibrary = new ValidationMatcherLibrary
        {
            Name = "fooValidationMatcherLibrary",
            Prefix = "foo:"
        };
        _validationMatcherLibrary.Members.Add("customMatcher", _matcher.Object);
    }

    [Test]
    public void TestGetValidationMatcherLibrary()
    {
        Context.ValidationMatcherRegistry.AddValidationMatcherLibrary(_validationMatcherLibrary);
        ClassicAssert.AreEqual(_validationMatcherLibrary,
            Context.ValidationMatcherRegistry.GetLibraryForPrefix("foo:"));
    }

    [Test]
    public void TestUnknownValidationMatcherLibrary()
    {
        try
        {
            Context.ValidationMatcherRegistry.GetLibraryForPrefix("unknown:");
        }
        catch (NoSuchValidationMatcherLibraryException e)
        {
            ClassicAssert.IsTrue(e.GetMessage().Contains("unknown:"));
        }
    }
}
