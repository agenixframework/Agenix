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

using Agenix.Api.Context;
using Agenix.Core;
using Agenix.Screenplay.Exceptions;

namespace Agenix.Screenplay.Abilities;

/// <summary>
///     An ability that provides actors with access to an IGherkinTestActionRunner,
///     enabling them to execute test cases and interact with the Agenix test execution framework.
/// </summary>
public class UseTheGherkinTestActionRunner : AbilityWithDefaultDescription, IRefersToActor
{
    /// <summary>
    ///     Initializes a new instance of IGherkinTestActionRunner with the specified test case runner.
    /// </summary>
    /// <param name="testCaseRunner">The IGherkinTestActionRunner to use.</param>
    public UseTheGherkinTestActionRunner(IGherkinTestActionRunner testCaseRunner)
    {
        TestCaseRunner = testCaseRunner ?? throw new ArgumentNullException(nameof(testCaseRunner));
    }

    /// <summary>
    ///     Initializes a new instance of UseTheTestCaseRunner by creating a runner from the test context.
    /// </summary>
    /// <param name="testContext">The TestContext to create the runner from.</param>
    public UseTheGherkinTestActionRunner(TestContext testContext)
    {
        if (testContext == null)
        {
            throw new ArgumentNullException(nameof(testContext));
        }

        TestCaseRunner = TestCaseRunnerFactory.CreateRunner(testContext);
    }

    /// <summary>
    ///     Gets the IGherkinTestActionRunner instance.
    /// </summary>
    public IGherkinTestActionRunner TestCaseRunner { get; }

    /// <summary>
    ///     Returns this ability as the specified type for the given actor.
    /// </summary>
    /// <typeparam name="T">The type of ability to return.</typeparam>
    /// <param name="actor">The actor using this ability.</param>
    /// <returns>This ability cast to the specified type.</returns>
    public T AsActor<T>(Actor actor) where T : IAbility
    {
        return (T)(IAbility)this;
    }

    /// <summary>
    ///     Gets the IGherkinTestActionRunner ability from the specified actor.
    /// </summary>
    /// <param name="actor">The actor to get the ability from.</param>
    /// <returns>The IGherkinTestActionRunner ability.</returns>
    /// <exception cref="NoMatchingAbilityException">Thrown when the actor doesn't have this ability.</exception>
    public static UseTheGherkinTestActionRunner As(Actor actor)
    {
        var ability = actor.AbilityTo<UseTheGherkinTestActionRunner>();
        if (ability == null)
        {
            throw new NoMatchingAbilityException(
                $"Actor '{actor.Name}' does not have the UseTheTestCaseRunner ability");
        }

        return ability.AsActor<UseTheGherkinTestActionRunner>(actor);
    }

    /// <summary>
    ///     Creates a new UseTheGherkinTestActionRunner ability with a test case runner created from the provided context.
    /// </summary>
    /// <param name="testContext">The TestContext to create the runner from.</param>
    /// <returns>A new UseTheGherkinTestActionRunner ability.</returns>
    public static UseTheGherkinTestActionRunner WithContext(TestContext testContext)
    {
        return new UseTheGherkinTestActionRunner(testContext);
    }

    /// <summary>
    ///     Creates a new UseTheGherkinTestActionRunner ability with a specific test case runner.
    /// </summary>
    /// <param name="testCaseRunner">The IGherkinTestActionRunner to use.</param>
    /// <returns>A new UseTheGherkinTestActionRunner ability.</returns>
    public static UseTheGherkinTestActionRunner WithRunner(IGherkinTestActionRunner testCaseRunner)
    {
        return new UseTheGherkinTestActionRunner(testCaseRunner);
    }
}
