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
    /// <summary>
    ///     Executes the specified performance logic for the given actor.
    ///     This method is typically overridden in derived classes to define
    ///     specific tasks or interactions that an actor can perform.
    /// </summary>
    /// <typeparam name="T">
    ///     The type of the actor who will execute the performance logic.
    ///     Usually, this corresponds to a class inheriting from <see cref="Actor" />.
    /// </typeparam>
    /// <param name="actor">
    ///     The instance of the actor that will perform the action. The actor
    ///     provides the necessary context to execute the defined performance logic.
    /// </param>
    public abstract void PerformAs<T>(T actor) where T : Actor;

    /// <summary>
    ///     Creates a <see cref="RememberThat.MemoryBuilder" /> using the specified memory key.
    ///     This method serves as the starting point for associating a memory key with either
    ///     a static value or a question evaluation in the screenplay testing framework.
    /// </summary>
    /// <param name="memoryKey">
    ///     The key used to identify the memory association. This value acts as an identifier
    ///     for the stored data or question result within the screenplay interaction.
    /// </param>
    /// <returns>
    ///     A <see cref="RememberThat.MemoryBuilder" /> instance initialized with the given memory key.
    ///     The returned builder allows fluent configuration of memory associations.
    /// </returns>
    public static MemoryBuilder TheValueOf(string memoryKey)
    {
        return new MemoryBuilder(memoryKey);
    }

    /// <summary>
    ///     Represents an action to store a specific value associated with a memory key in the context of a screenplay-style
    ///     interaction.
    ///     Enables an actor to retain particular information by associating it with a predefined key, supporting assertions
    ///     and
    ///     interactivity within the framework.
    /// </summary>
    public class WithValue(string memoryKey, object value) : RememberThat
    {
        /// <summary>
        ///     Executes the specified performance logic for the given actor.
        ///     This method is responsible for enabling the actor to recall or store
        ///     a memory value based on the context defined in the derived classes.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the actor who will execute the performance logic.
        ///     Typically, this corresponds to a class capable of interacting with
        ///     the screenplay framework, such as an instance implementing memory management.
        /// </typeparam>
        /// <param name="actor">
        ///     The actor instance that carries out the defined memory-related actions.
        ///     The actor provides the necessary context and methods to store or retrieve memory.
        /// </param>
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
        /// <summary>
        ///     Executes the performance logic defined by the derived class
        ///     using the provided actor as context.
        ///     This method enables the actor to perform specific tasks or interactions
        ///     associated with the screenplay framework.
        /// </summary>
        /// <typeparam name="T">
        ///     The type of the actor executing the performance logic.
        ///     This is typically a class that represents an actor in the screenplay pattern.
        /// </typeparam>
        /// <param name="actor">
        ///     The actor instance that provides the context and capability
        ///     for performing the defined logic. The actor interacts with
        ///     the screenplay environment during execution.
        /// </param>
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
