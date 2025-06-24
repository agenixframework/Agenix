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

using System;
using Agenix.Api.Annotations;

namespace Agenix.Core.Endpoint.Direct.Annotation;

/// <summary>
///     An attribute to configure the properties of a direct sync endpoint for
///     use within the Agenix.Core.Config.Annotation namespace.
/// </summary>
/// <remarks>
///     This attribute is used to configure a direct sync endpoint with properties
///     such as the qualifier, queue name, polling interval, correlator, and timeout.
/// </remarks>
[AttributeUsage(AttributeTargets.Field)]
[AgenixEndpointConfig("direct.sync")]
public class DirectSyncEndpointConfigAttribute : Attribute
{
    /// <summary>
    ///     Qualifier for the endpoint configuration.
    /// </summary>
    public string Qualifier { get; set; } = "direct.sync";

    /// <summary>
    ///     Destination name.
    /// </summary>
    public string QueueName { get; set; } = string.Empty;

    /// <summary>
    ///     Destination reference.
    /// </summary>
    public string Queue { get; set; } = string.Empty;

    /// <summary>
    ///     Polling interval.
    /// </summary>
    public int PollingInterval { get; set; } = 500;

    /// <summary>
    ///     Message correlator.
    /// </summary>
    public string Correlator { get; set; } = string.Empty;

    /// <summary>
    ///     Timeout.
    /// </summary>
    public long Timeout { get; set; } = 5000L;
}
