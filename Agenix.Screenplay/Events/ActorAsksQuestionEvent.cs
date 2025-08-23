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

namespace Agenix.Screenplay.Events;

/// <summary>
///     Represents the event where an actor asks a question within the screenplay pattern.
///     This class is used to capture the interaction of an actor asking a specific question and obtaining an answer.
/// </summary>
/// <typeparam name="T">The type of the answer expected from the question being asked.</typeparam>
public class ActorAsksQuestion : ActorPerformanceEvent, IActorAsksQuestionEvent
{
    /// <summary>
    ///     Represents the event where an actor asks a question within the screenplay pattern.
    ///     Handles the interaction between an actor and a question they seek an answer for.
    /// </summary>
    /// <typeparam name="T">The type of the answer that the question provides.</typeparam>
    public ActorAsksQuestion(string subject, string actor) : base(actor)
    {
        QuestionSubject = subject;
    }

    // Implement the non-generic interface
    /// <summary>
    ///     Gets the name of the actor associated with the event.
    /// </summary>
    public string ActorName => Name;

    /// <summary>
    ///     Gets the subject of the question being asked by the actor.
    /// </summary>
    public string QuestionSubject { get; }
}
