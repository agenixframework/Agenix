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

using Agenix.Core.Endpoint.Builder;

namespace Agenix.Selenium.Endpoint.Builder;

/// <summary>
///     Static factory class for creating Selenium endpoint builders.
///     Provides a convenient entry point for building Selenium browser endpoints using a fluent API.
/// </summary>
public sealed class SeleniumEndpoints : AbstractEndpointBuilder<SeleniumBrowserBuilder>
{
    /// <summary>
    ///     Private constructor using browser builder implementation
    /// </summary>
    private SeleniumEndpoints() : base(new SeleniumBrowserBuilder())
    {
    }

    /// <summary>
    ///     Static entry method for Selenium endpoints
    /// </summary>
    /// <returns>A new SeleniumEndpoints instance</returns>
    public static SeleniumEndpoints Selenium()
    {
        return new SeleniumEndpoints();
    }

    /// <summary>
    ///     Returns browser builder for further fluent API calls
    /// </summary>
    /// <returns>The SeleniumBrowserBuilder instance</returns>
    public SeleniumBrowserBuilder Browser()
    {
        return _builder;
    }
}
