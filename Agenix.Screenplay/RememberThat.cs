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
///     Represents the ability to associate specific data or answers with a memory key,
///     enabling an actor in a screenplay-style interaction to retain and recall information.
///     Provides fluent methods for defining and storing memory associations, supporting
///     agent-based interactions and assertions within the screenplay framework.
/// </summary>
public abstract class RememberThat : IPerformable
{
    public abstract void PerformAs<T>(T actor) where T : Actor;

    public static MemoryBuilder TheValueOf(string memoryKey)
    {
        return new MemoryBuilder(memoryKey);
    }

    public class WithValue(string memoryKey, object value) : RememberThat
    {
        public override void PerformAs<T>(T actor)
        {
            actor.Remember(memoryKey, value);
        }
    }

    /// <summary>
    ///     Represents the ability to associate an answer obtained from a question
    ///     with a specific memory key, enabling an actor to remember the result for later use.
    ///     This class supports fluent memory management for screenplay-style interactions.
    /// </summary>
    public class WithQuestion(string memoryKey, IQuestion<dynamic> question) : RememberThat
    {
        public override void PerformAs<T>(T actor)
        {
            actor.Remember(memoryKey, question.AnsweredBy(actor));
        }
    }

    /// <summary>
    ///     Represents a builder for creating memory associations by specifying a memory key
    ///     and associating it with a value or a question. This class is part of the agent-based
    ///     testing framework to enable fluent actions using memory concepts.
    /// </summary>
    public class MemoryBuilder(string memoryKey)
    {
        /// <summary>
        ///     Associates the provided value with the memory key specified during the creation
        ///     of the <see cref="MemoryBuilder" /> instance. This defines a memory association
        ///     that can later be used by the agent-based testing framework.
        /// </summary>
        /// <param name="value">
        ///     The value to be associated with the memory key. This value represents the data
        ///     to be remembered for later usage or assertions.
        /// </param>
        /// <returns>
        ///     A <see cref="RememberThat" /> instance configured with the specified memory key
        ///     and value.
        /// </returns>
        public RememberThat Is(object value)
        {
            return Instrumented.InstanceOf<WithValue>()
                .WithProperties(memoryKey, value);
        }

        /// <summary>
        ///     Creates a memory association by linking the specified memory key with a question.
        ///     This enables the agent-based testing framework to resolve the question dynamically
        ///     during execution.
        /// </summary>
        /// <param name="value">
        ///     The question to be associated with the memory key. This question is used to
        ///     retrieve or compute the value dynamically at runtime.
        /// </param>
        /// <returns>
        ///     A <see cref="RememberThat" /> instance configured with the specified memory key
        ///     and the provided question.
        /// </returns>
        public RememberThat IsAnsweredBy(IQuestion<dynamic> value)
        {
            return Instrumented.InstanceOf<WithQuestion>()
                .WithProperties(memoryKey, value);
        }
    }
}
