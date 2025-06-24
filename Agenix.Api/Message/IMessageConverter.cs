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
using Agenix.Api.Endpoint;

namespace Agenix.Api.Message;

/// Interface defining methods for converting between external and internal message types.
/// It supports converting an internal message to an external message for outbound communication,
/// and converting an external message to an internal message for inbound processing.
/// @param
/// <I>
///     The type representing the external message for inbound conversion.
///     @param
///     <O>
///         The type representing the external message for outbound conversion.
///         @param
///         <C>
///             The type representing the endpoint configuration, which must implement IEndpointConfiguration.
///             /
public interface IMessageConverter<in I, O, in C> where C : IEndpointConfiguration
{
    /// Converts an internal message to an external message for outbound communication.
    /// @param internalMessage The internal message that needs to be converted to an external format.
    /// @param endpointConfiguration The endpoint configuration that provides necessary context for the conversion process.
    /// @param context The test context containing additional data or state information required during the conversion.
    /// @return The converted external message suitable for outbound communication.
    /// /
    O ConvertOutbound(IMessage internalMessage, C endpointConfiguration, TestContext context);

    /// Converts an internal message to an external message for outbound communication.
    /// <param name="externalMessage">
    ///     The external message object that will be enriched with information from the internal
    ///     message.
    /// </param>
    /// <param name="internalMessage">The internal message that provides data for the conversion process.</param>
    /// <param name="endpointConfiguration">The endpoint configuration that provides necessary context for the conversion.</param>
    /// <param name="context">The test context containing additional data or state information required during the conversion.</param>
    void ConvertOutbound(O externalMessage, IMessage internalMessage, C endpointConfiguration, TestContext context);

    /// Converts an external message to an internal representation for inbound processing.
    /// <param name="externalMessage">The external message that needs to be converted to an internal format.</param>
    /// <param name="endpointConfiguration">The endpoint configuration providing necessary context for the conversion process.</param>
    /// <param name="context">The test context containing additional data or state information required during the conversion.</param>
    /// <return>The converted internal message suitable for processing within the system.</return>
    IMessage ConvertInbound(I externalMessage, C endpointConfiguration, TestContext context);
}
