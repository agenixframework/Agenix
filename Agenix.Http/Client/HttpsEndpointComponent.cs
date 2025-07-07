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

namespace Agenix.Http.Client;

/// <summary>
///     Component creates proper HTTP client endpoint from endpoint uri resource and parameters.
/// </summary>
public class HttpsEndpointComponent() : HttpEndpointComponent("https")
{
    /// <summary>
    /// Gets the scheme that specifies the protocol used for constructing endpoint URIs in the derived HTTP client component.
    /// The property returns a predefined scheme value, such as "https://" for secure connections in derived classes like <c>HttpsEndpointComponent</c>.
    /// This serves as the base URI protocol scheme for endpoints created by the component.
    /// </summary>
    protected override string Scheme => "https://";
}
