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
using Agenix.Core.Actions;
using NUnit.Framework;
using static Agenix.Core.Container.AssertContainer.Builder;
using Assert = NUnit.Framework.Assert;

namespace Agenix.Core.Tests.Container;

public class AssertTest : AbstractNUnitSetUp
{
    [Test]
    public void TestAssertDefaultException()
    {
        var assertAction = Assert()
            .Actions(new FailAction.Builder())
            .Build();

        // Assert that execution completes without throwing (the container handles the expected failure internally)
        Assert.That(async () => await assertAction.ExecuteAsync(Context), Throws.Nothing);
    }

    [Test]
    public void TestAssertException()
    {
        var exceptionType = typeof(AgenixSystemException);

        var assertAction = Assert()
            .Actions(new FailAction.Builder())
            .Exception(exceptionType)
            .Build();

        Assert.That(async () => await assertAction.ExecuteAsync(Context), Throws.Nothing);
    }

    [Test]
    public void TestAssertExceptionMessageCheck()
    {
        var failActionBuilder = new FailAction.Builder()
            .WithMessage("This went wrong!");

        var exceptionType = typeof(AgenixSystemException);

        var assertAction = Assert()
            .Actions(failActionBuilder)
            .Exception(exceptionType)
            .Message("This went wrong!")
            .Build();

        Assert.That(async () => await assertAction.ExecuteAsync(Context), Throws.Nothing);
    }

    [Test]
    public void TestVariableSupport()
    {
        Context.SetVariable("message", "This went wrong!");

        var failActionBuilder = new FailAction.Builder()
            .WithMessage("This went wrong!");

        var exceptionType = typeof(AgenixSystemException);

        var assertAction = Assert()
            .Actions(failActionBuilder)
            .Exception(exceptionType)
            .Message(Context.GetVariable("${message}"))
            .Build();

        Assert.That(async () => await assertAction.ExecuteAsync(Context), Throws.Nothing);
    }

    [Test]
    public void TestValidationMatcherSupport()
    {
        var failActionBuilder = new FailAction.Builder()
            .WithMessage("This went wrong!");

        var exceptionType = typeof(AgenixSystemException);

        var assertAction = Assert()
            .Actions(failActionBuilder)
            .Exception(exceptionType)
            .Message("@Contains('wrong')@")
            .Build();

        Assert.That(async () => await assertAction.ExecuteAsync(Context), Throws.Nothing);
    }

    [Test]
    public void TestAssertExceptionWrongMessageCheck()
    {
        var failActionBuilder = new FailAction.Builder().WithMessage("This went wrong!");

        var exceptionType = typeof(AgenixSystemException);

        var assertAction = Assert()
            .Actions(failActionBuilder)
            .Exception(exceptionType)
            .Message("Expected error is something else")
            .Build();

        // Expect validation to fail with the mismatching message
        Assert.That(async () => await assertAction.ExecuteAsync(Context),
            Throws.TypeOf<ValidationException>().With.Message.Contain("Expected error is something else"));
    }

    [Test]
    public void TestMissingException()
    {
        var exceptionType = typeof(AgenixSystemException);

        var assertAction = Assert()
            .Actions(new EchoAction.Builder())
            .Exception(exceptionType)
            .Build();

        // The expected exception is missing, so executing the assert action should throw
        Assert.That(async () => await assertAction.ExecuteAsync(Context), Throws.TypeOf<ValidationException>());
    }
}
