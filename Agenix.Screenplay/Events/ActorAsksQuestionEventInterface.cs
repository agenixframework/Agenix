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

using MediatR;

namespace Agenix.Screenplay.Events;

/// <summary>
///     Represents an event where an actor queries a specific question within a screenplay context.
///     This event is used to capture the details of the actor's inquiry, including the actor's identity,
///     the subject of the question, and the timestamp when the event occurred.
/// </summary>
public interface IActorAsksQuestionEvent : INotification
{
    string ActorName { get; }
    string QuestionSubject { get; }
    DateTime Timestamp { get; }
}
