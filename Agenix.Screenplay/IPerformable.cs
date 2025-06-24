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

namespace Agenix.Screenplay;

/**
 * A task or action that can be performed by an actor.
 *
 *
 * It is common to use builder methods to create instances of the Performable class, in order to make the tests read
 * more fluently. For example:
 *
 * [source,c#]
 * --
 * purchase().anApple().thatCosts(0).dollars()
 * --
 */
public interface IPerformable
{
    /// <summary>
    ///     Performs the specified action or task using the provided actor.
    /// </summary>
    /// <typeparam name="T">The type of actor performing the action.</typeparam>
    /// <param name="actor">The actor that will perform the action or task.</param>
    void PerformAs<T>(T actor) where T : Actor;

    /// <summary>
    ///     Chains the current performable to another performable, creating a composite performable that will execute both in
    ///     sequence.
    /// </summary>
    /// <param name="nextPerformable">The next performable to be executed.</param>
    /// <returns>A composite performable combining the current and the next performable.</returns>
    IPerformable Then(IPerformable nextPerformable)
    {
        return CompositePerformable.From(this, nextPerformable);
    }
}
