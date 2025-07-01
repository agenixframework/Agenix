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

using Agenix.Api.Annotations;

namespace Agenix.Selenium.Config;

/// <summary>
/// Configuration attribute for Selenium browser endpoints
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
[AgenixEndpointConfig("selenium.browser")]
public class SeleniumBrowserConfigAttribute : Attribute
{
    /// <summary>
    /// Browser start page
    /// </summary>
    public string StartPage { get; set; } = "";

    /// <summary>
    /// Version
    /// </summary>
    public string Version { get; set; } = "";

    /// <summary>
    /// Remote server URL
    /// </summary>
    public string RemoteServer { get; set; } = "";

    /// <summary>
    /// Browser event listeners
    /// </summary>
    public string[] EventListeners { get; set; } = [];

    /// <summary>
    /// Web driver instance
    /// </summary>
    public string WebDriver { get; set; } = "";

    /// <summary>
    /// Browser type
    /// </summary>
    public string Type { get; set; } = "";

    /// <summary>
    /// Firefox profile
    /// </summary>
    public string FirefoxProfile { get; set; } = "";

    /// <summary>
    /// JavaScript enabled
    /// </summary>
    public bool JavaScript { get; set; } = true;

    /// <summary>
    /// Timeout in milliseconds
    /// </summary>
    public long Timeout { get; set; } = 5000L;
}
