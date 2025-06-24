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

using Agenix.Api.Context;

namespace Agenix.Api.Message;

/// Interface for transforming a message within a specified test context.
public interface IMessageTransformer
{
    /// Transform a message with a given test context and return a new message.
    /// @param message the message to process.
    /// @param context the current test context.
    /// @return the processed message.
    /// /
    extern IMessage Transform(IMessage message, TestContext context);

    /// Defines a generic builder interface for constructing instances of IMessageTransformer.
    /// @type param T The type of IMessageTransformer that this builder will create.
    /// @type param TB The type of the builder itself, to allow for fluent builder patterns.
    /// /
    interface IBuilder<out T, TB> where T : IMessageTransformer
        where TB : IBuilder
    {
        /// Builds a new message processor instance.
        /// @return new instance of message processor.
        T Build();
    }
}
