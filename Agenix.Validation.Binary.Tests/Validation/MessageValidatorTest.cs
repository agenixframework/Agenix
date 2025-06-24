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
using Agenix.Api.Validation.Context;
using Agenix.Core.Validation;
using Agenix.Validation.Binary.Validation;
using NUnit.Framework;

namespace Agenix.Validation.Binary.Tests.Validation;

/// <summary>
///     A test suite for validating the functionality and behavior of message validators.
///     This class is designed to verify the correct lookup and configuration of message validators
///     in the Agenix application, ensuring their proper registration and implementation.
/// </summary>
public class MessageValidatorTest
{
    [Test]
    public void ShouldLookupValidators()
    {
        //WHEN
        var validators = IMessageValidator<IValidationContext>.Lookup();

        //THEN
        Assert.That(validators, Has.Count.EqualTo(2));

        Assert.That(validators["header"], Is.Not.Null);
        Assert.That(validators["header"],
            Is.TypeOf<DefaultMessageHeaderValidator>());

        Assert.That(validators["binary"], Is.Not.Null);
        Assert.That(validators["binary"],
            Is.TypeOf<BinaryMessageValidator>());
    }

    [Test]
    public void ShouldLookupSpecificValidators()
    {
        //WHEN/THEN
        Assert.That(IMessageValidator<IValidationContext>.Lookup("header").IsPresent, Is.True);
        Assert.That(IMessageValidator<IValidationContext>.Lookup("binary").IsPresent, Is.True);
    }
}
