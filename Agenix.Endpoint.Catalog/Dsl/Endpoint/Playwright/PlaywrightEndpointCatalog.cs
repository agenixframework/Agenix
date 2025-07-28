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
using Agenix.Playwright.Endpoint;
using Agenix.Playwright.Endpoint.Builder;

namespace Agenix.Endpoint.Catalog.Dsl.Endpoint.Playwright;

/// <summary>
///     Static catalog class for creating Playwright endpoint builders.
///     Provides convenient access to Playwright browser functionality.
/// </summary>
public class PlaywrightEndpointCatalog
{
    /// <summary>
    ///     Private constructor to prevent direct instantiation
    /// </summary>
    private PlaywrightEndpointCatalog()
    {
        // prevent direct instantiation
    }

    /// <summary>
    ///     Static entry method for Playwright endpoint catalog
    /// </summary>
    /// <returns>A new PlaywrightEndpointCatalog instance</returns>
    public static PlaywrightEndpointCatalog Playwright()
    {
        return new PlaywrightEndpointCatalog();
    }

    /// <summary>
    ///     Gets the browser builder
    /// </summary>
    /// <returns>The PlaywrightBrowserBuilder instance</returns>
    [SuppressMessage("csharpsquid", "S2325", Justification = "Intentionally non-static for fluent API pattern")]
    public PlaywrightBrowserBuilder Browser()
    {
        return PlaywrightEndpoints.Playwright().Browser();
    }
}
