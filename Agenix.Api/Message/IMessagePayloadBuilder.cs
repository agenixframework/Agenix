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

public delegate object MessagePayloadBuilder(TestContext context);

/// <summary>
///     Provides an interface for building message payloads using a test context.
/// </summary>
public interface IMessagePayloadBuilder
{
    /// <summary>
    ///     Builds the payload for a message using the provided test context.
    /// </summary>
    /// <param name="context">The test context used for building the payload.</param>
    /// <returns>An object representing the built payload.</returns>
    object BuildPayload(TestContext context);

    /// <summary>
    ///     Provides an interface for building message payloads using a test context.
    /// </summary>
    /// <typeparam name="T">The type of the payload builder.</typeparam>
    /// <typeparam name="TB">The type of the builder itself.</typeparam>
    public interface IBuilder<out T, TB>
        where T : IMessagePayloadBuilder
        where TB : IBuilder<T, TB>
    {
        /// <summary>
        ///     Builds the payload for a message using the provided test context.
        /// </summary>
        /// <returns>An object representing the built payload.</returns>
        T Build();
    }
}
