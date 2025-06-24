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

/// <summary>
///     Represents an interface for entities capable of performing tasks and asking questions.
/// </summary>
public interface IPerformsTasks
{
    /// <summary>
    ///     Allows an actor to perform one or more tasks or actions represented by the <see cref="IPerformable" /> interface.
    /// </summary>
    /// <typeparam name="T">The type of tasks or actions to be performed, which must implement <see cref="IPerformable" />.</typeparam>
    /// <param name="todos">A collection of tasks or actions that the actor will attempt to perform.</param>
    void AttemptsTo<T>(params T[] todos) where T : IPerformable;

    /// <summary>
    ///     Allows an actor to request an answer to a specified question represented by the <see cref="IQuestion{TAnswer}" />
    ///     interface.
    /// </summary>
    /// <typeparam name="ANSWER">The type of the answer expected from the question.</typeparam>
    /// <param name="question">The question from which the actor seeks an answer.</param>
    /// <returns>The result of the question, which is of type <typeparamref name="ANSWER" />.</returns>
    ANSWER AsksFor<ANSWER>(IQuestion<ANSWER> question);
}
