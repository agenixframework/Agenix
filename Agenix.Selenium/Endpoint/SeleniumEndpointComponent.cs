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
using Agenix.Core.Endpoint;

namespace Agenix.Selenium.Endpoint;

/// <summary>
///     Selenium endpoint component for creating and configuring Selenium browser endpoints.
///     This component handles the creation of SeleniumBrowser instances based on resource paths and parameters.
/// </summary>
public class SeleniumEndpointComponent : AbstractEndpointComponent
{
    /// <summary>
    ///     Default constructor using the name for this component
    /// </summary>
    public SeleniumEndpointComponent() : base("selenium")
    {
    }

    /// <summary>
    ///     Creates a new Selenium browser endpoint based on the provided resource path and parameters
    /// </summary>
    /// <param name="resourcePath">The resource path that may specify browser type</param>
    /// <param name="parameters">Configuration parameters for the endpoint</param>
    /// <param name="context">The test context</param>
    /// <returns>A configured SeleniumBrowser endpoint</returns>
    protected override IEndpoint CreateEndpoint(string resourcePath, IDictionary<string, string> parameters,
        TestContext context)
    {
        var browser = new SeleniumBrowser();

        // Set the browser type from a resource path if specified and not the default "browser"
        if (!string.IsNullOrWhiteSpace(resourcePath) &&
            !resourcePath.Equals("browser", StringComparison.OrdinalIgnoreCase))
        {
            browser.EndpointConfiguration.BrowserType = resourcePath;
        }

        // Configure the start page URL if provided
        if (parameters.TryGetValue("start-page", out var startPage))
        {
            browser.EndpointConfiguration.StartPageUrl = startPage;
            parameters.Remove("start-page");
        }

        // Configure a remote server URL if provided
        if (parameters.TryGetValue("remote-server", out var remoteServer))
        {
            browser.EndpointConfiguration.RemoteServerUrl = remoteServer;
            parameters.Remove("remote-server");
        }

        // Enrich endpoint configuration with remaining parameters
        EnrichEndpointConfiguration(browser.EndpointConfiguration,
            GetEndpointConfigurationParameters(parameters, typeof(SeleniumBrowserConfiguration)), context);

        return browser;
    }
}
