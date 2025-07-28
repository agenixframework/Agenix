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

namespace Agenix.Screenplay.Conditions;

/// <summary>
///     Represents an abstract base class for conditionally performing tasks or actions based on
///     an evaluated condition. It allows specifying tasks to execute for true and false outcomes
///     of the condition when implemented.
/// </summary>
public abstract class ConditionalPerformable : IPerformable
{
    private readonly Dictionary<bool, IPerformable[]> _outcomeToPerform;

    /// <summary>
    ///     Represents a base class for conditionally defining and performing tasks or actions
    ///     based on a specific condition. Concrete implementations must provide an evaluation
    ///     logic to determine the condition outcome that dictates which set of tasks to execute.
    /// </summary>
    protected ConditionalPerformable()
    {
        _outcomeToPerform = new Dictionary<bool, IPerformable[]>
        {
            { true, [new AnonymousPerformable()] }, { false, [new AnonymousPerformable()] }
        };
    }

    /// <summary>
    ///     Executes the set of tasks associated with the evaluated result of the condition
    ///     for the given actor. The tasks to perform depend on the true or false outcome
    ///     of the evaluation for the specified actor.
    /// </summary>
    /// <typeparam name="T">The type of actor performing the tasks; must inherit from the Actor class.</typeparam>
    /// <param name="actor">The actor for whom the condition is evaluated, and who performs the corresponding tasks.</param>
    public void PerformAs<T>(T actor) where T : Actor
    {
        actor.AttemptsTo(_outcomeToPerform[EvaluatedConditionFor(actor)]);
    }

    /// <summary>
    ///     Specifies a set of tasks to execute if the evaluated condition is true.
    /// </summary>
    /// <param name="taskToPerform">An array of tasks or actions to be performed when the condition evaluates to true.</param>
    /// <returns>The current instance of <see cref="ConditionalPerformable" />, allowing method chaining.</returns>
    public ConditionalPerformable AndIfSo(params IPerformable[] taskToPerform)
    {
        _outcomeToPerform[true] = taskToPerform;
        return this;
    }

    /// <summary>
    ///     Specifies tasks to execute when the condition evaluates to false.
    /// </summary>
    /// <param name="taskToPerform">The tasks to perform if the condition outcome is false.</param>
    /// <returns>The updated instance of the conditional performable with tasks assigned for the false condition.</returns>
    public ConditionalPerformable Otherwise(params IPerformable[] taskToPerform)
    {
        _outcomeToPerform[false] = taskToPerform;
        return this;
    }

    /// <summary>
    ///     Evaluates a condition based on the specific state or attributes of the provided actor.
    ///     The result of this evaluation determines which tasks or actions should be performed.
    /// </summary>
    /// <param name="actor">The actor whose state or attributes are evaluated to determine the condition outcome.</param>
    /// <returns>
    ///     A boolean indicating the result of the evaluated condition. Returns true if the condition is met; otherwise,
    ///     false.
    /// </returns>
    protected abstract bool EvaluatedConditionFor(Actor actor);
}
