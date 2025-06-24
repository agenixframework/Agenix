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

/// <summary>
///     Interface representing a builder for constructing messages.
/// </summary>
public interface IMessageBuilder
{
    IMessage Build(TestContext context, string messageType);
}

/// <summary>
///     Define a delegate for creating <see cref="IMessage" /> objects based on the provided
///     <see cref="TestContext" /> and message type.
/// </summary>
/// <param name="context">
///     Instance of <see cref="TestContext" /> which provides utility methods and state
///     for generating messages.
/// </param>
/// <param name="messageType">A string indicating the type of message to be created.</param>
/// <returns>An instance of <see cref="IMessage" /> based on the specified context and message type.</returns>
public delegate IMessage MessageBuilder(TestContext context, string messageType);
