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
///     Represents a question in the Screenplay pattern to verify the presence of a specific memory
///     associated with an <see cref="Actor" />.
/// </summary>
public class TheMemory : IQuestion<bool>
{
    private readonly string _memoryKey;

    private TheMemory(string memoryKey)
    {
        _memoryKey = memoryKey;
    }

    public bool AnsweredBy(Actor actor)
    {
        return actor.Recall<dynamic>(_memoryKey) != null;
    }

    /// <summary>
    ///     Initializes a builder to create a memory question with the specified memory key.
    /// </summary>
    /// <param name="memoryKey">The key to identify the memory to be questioned.</param>
    /// <returns>A <see cref="TheMemoryQuestionBuilder" /> instance initialized with the specified memory key.</returns>
    public static TheMemoryQuestionBuilder WithKey(string memoryKey)
    {
        return new TheMemoryQuestionBuilder(memoryKey);
    }

    /// <summary>
    ///     Provides a builder for creating instances of <see cref="TheMemory" /> questions.
    /// </summary>
    public class TheMemoryQuestionBuilder(string memoryKey)
    {
        public TheMemory IsPresent()
        {
            return new TheMemory(memoryKey);
        }
    }
}
