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

using Agenix.Api.Message;

namespace Agenix.Api.Endpoint.Resolver;

/// <summary>
///     Resolves endpoint uri so we can send messages to dynamic endpoints. Resolver works on request message and chooses
///     the target message endpoint according to message headers or payload.
/// </summary>
public interface IEndpointUriResolver
{
    /// <summary>
    ///     Static header entry name specifying the dynamic endpoint URI.
    /// </summary>
    static readonly string EndpointUriHeaderName = MessageHeaders.Prefix + "endpoint_uri";

    static readonly string RequestPathHeaderName = MessageHeaders.Prefix + "request_path";
    static readonly string QueryParamHeaderName = MessageHeaders.Prefix + "query_params";

    /// <summary>
    ///     Get the dedicated message endpoint URI for this message.
    /// </summary>
    /// <param name="message">The request message to send.</param>
    /// <param name="defaultUri">The fallback URI in case no mapping was found.</param>
    /// <returns>The endpoint URI string representation.</returns>
    string ResolveEndpointUri(IMessage message, string defaultUri);
}
