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

using System.Diagnostics.CodeAnalysis;
using Agenix.Selenium.Endpoint;
using Agenix.Selenium.Endpoint.Builder;

namespace Agenix.Endpoint.Catalog.Dsl.Endpoint.Selenium;

/// <summary>
///     Static catalog class for creating Selenium endpoint builders.
///     Provides convenient access to Selenium browser functionality.
/// </summary>
public class SeleniumEndpointCatalog
{
    /// <summary>
    ///     Private constructor setting the client and server builder implementation.
    /// </summary>
    private SeleniumEndpointCatalog()
    {
        // prevent direct instantiation
    }

    /// <summary>
    ///     Static entry method for Selenium endpoint catalog
    /// </summary>
    /// <returns>A new SeleniumEndpointCatalog instance</returns>
    public static SeleniumEndpointCatalog Selenium()
    {
        return new SeleniumEndpointCatalog();
    }

    /// <summary>
    ///     Gets the browser builder.
    /// </summary>
    /// <returns>The SeleniumBrowserBuilder instance</returns>
    [SuppressMessage("csharpsquid", "S2325", Justification = "Intentionally non-static for fluent API pattern")]
    public SeleniumBrowserBuilder Browser()
    {
        return SeleniumEndpoints.Selenium().Browser();
    }
}
