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

using Agenix.Core;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using Agenix.Screenplay.Fact;
using NUnit.Framework;

namespace Agenix.Screenplay.Tests.Facts;

[NUnitAgenixSupport]
public class WhenDeclaringFactsAboutActors
{
    private static readonly List<string> KnownAccounts = new();

    private static readonly List<string> ExistingSavingsAccounts = [];

    [Test]
    public async Task Facts_Let_Us_Prepare_An_Actor_For_A_Scenario()
    {
        // Arrange
        var tim = Actor.Named("Tim");

        // Act
        await tim.Has(AnAccount.Numbered("123456"));

        // Assert
        Assert.That(KnownAccounts, Does.Contain("123456"));
    }

    [Test]
    public async Task Facts_Let_Us_Setup_And_Teardown_Test_Data()
    {
        // Arrange
        var tim = Actor.Named("Tim");

        // Act
        await tim.Has(ASavingsAccount.Numbered("Savings-123456"));

        await AgenixInstanceManager.GetOrDefault().AgenixContext.TestListeners.OnTestFinish(new DefaultTestCase());

        // Assert
        Assert.That(KnownAccounts, Does.Contain("Savings-123456"));
        Assert.That(ExistingSavingsAccounts, Does.Not.Contain("Savings-123456"));

        await tim.WrapUp();
    }

    [Test]
    public void Facts_Can_Throw_Exceptions()
    {
        // Arrange
        var tim = Actor.Named("Tim");

        // Act & Assert
        Assert.ThrowsAsync<TestCompromisedException>(() => tim.Has(AnAccount.ThatIsIllegal()));
    }

    public class AnAccount(string accountNumber) : IFact
    {
        public Task Setup(Actor actor)
        {
            KnownAccounts.Add(accountNumber);

            return Task.CompletedTask;
        }

        public static IFact Numbered(string accountNumber)
        {
            return new AnAccount(accountNumber);
        }

        public static IFact ThatIsIllegal()
        {
            return new IllegalAccount();
        }

        public override string ToString()
        {
            return $"An account numbered {accountNumber}";
        }
    }

    private class IllegalAccount : IFact
    {
        public Task Setup(Actor actor)
        {
            throw new TestCompromisedException("Illegal account");
        }

        public override string ToString()
        {
            return "An illegal account";
        }
    }

    private class ASavingsAccount(string accountNumber) : IFact
    {
        public Task Setup(Actor actor)
        {
            KnownAccounts.Add(accountNumber);
            ExistingSavingsAccounts.Add(accountNumber);

            return Task.CompletedTask;
        }

        public Task Teardown(Actor actor)
        {
            ExistingSavingsAccounts.Remove(accountNumber);
            return Task.CompletedTask;
        }

        public static IFact Numbered(string accountNumber)
        {
            return new ASavingsAccount(accountNumber);
        }
    }


    public class TestCompromisedException(string message) : Exception(message);
}
