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
///     Represents a consequence that can be evaluated within the context of the Screenplay pattern.
/// </summary>
/// <typeparam name="T">The type of value associated with the consequence.</typeparam>
public interface IConsequence<T>
{
    void EvaluateFor(Actor actor);
    IConsequence<T> OrComplainWith(Type complaintType);
    IConsequence<T> OrComplainWith(Type complaintType, string complaintDetails);
    IConsequence<T> WhenAttemptingTo(IPerformable performable);
    IConsequence<T> Because(string explanation);

    /// <summary>
    ///     Evaluate the consequence only after performing the specified tasks.
    /// </summary>
    IConsequence<T> After(params IPerformable[] setupActions);
}
