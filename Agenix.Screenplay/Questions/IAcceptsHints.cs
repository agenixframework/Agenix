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

namespace Agenix.Screenplay.Questions;

/// <summary>
///     Represents an interface for objects that can accept hints derived from the <see cref="IQuestionHint" /> interface.
///     This is typically implemented by entities requiring additional context or metadata to enhance their behavior.
/// </summary>
public interface IAcceptsHints
{
    /// <summary>
    ///     Applies a set of hints to the implementing object. This method is designed for entities
    ///     that can accept hints derived from the <see cref="IQuestionHint" /> interface.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of hints being applied, which must implement the <see cref="IQuestionHint" /> interface.
    /// </typeparam>
    /// <param name="hints">
    ///     A set of hints to be applied. These hints provide additional context or information
    ///     used by the implementing object.
    /// </param>
    void Apply<T>(ISet<T> hints) where T : IQuestionHint;
}
