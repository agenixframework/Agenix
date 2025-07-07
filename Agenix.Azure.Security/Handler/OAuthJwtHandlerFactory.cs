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

using Agenix.Azure.Security.Client;
using Agenix.Azure.Security.Configuration;

namespace Agenix.Azure.Security.Handler;

/// <summary>
/// Factory for creating OAuth JWT authentication handlers
/// </summary>
public static class OAuthJwtHandlerFactory
{
    /// <summary>
    /// Create OAuth JWT handler with client credentials flow
    /// </summary>
    public static OAuthJwtAuthenticationHandler CreateClientCredentialsHandler(
        string tokenEndpoint,
        string clientId,
        string clientSecret,
        string[]? scopes = null,
        OAuthJwtHandlerOptions? options = null)
    {
        var oauthConfig = new OAuthClientConfiguration
        {
            TokenEndpoint = tokenEndpoint,
            ClientId = clientId,
            ClientSecret = clientSecret,
            Scopes = scopes?.ToList() ?? []
        };

        var tokenClient = new OAuthTokenClient();
        return new OAuthJwtAuthenticationHandler(tokenClient, oauthConfig, options);
    }

    /// <summary>
    /// Create OAuth JWT handler with full configuration
    /// </summary>
    public static OAuthJwtAuthenticationHandler CreateHandler(
        OAuthClientConfiguration oauthConfiguration,
        OAuthJwtHandlerOptions? options = null)
    {
        var tokenClient = new OAuthTokenClient();
        return new OAuthJwtAuthenticationHandler(tokenClient, oauthConfiguration, options);
    }

    /// <summary>
    /// Create OAuth JWT handler with custom HTTP client
    /// </summary>
    public static OAuthJwtAuthenticationHandler CreateHandler(
        OAuthClientConfiguration oauthConfiguration,
        HttpClient httpClient,
        OAuthJwtHandlerOptions? options = null)
    {
        var tokenClient = new OAuthTokenClient(httpClient);
        return new OAuthJwtAuthenticationHandler(tokenClient, oauthConfiguration, options);
    }

    /// <summary>
    /// Create OAuth JWT handler for client credentials with single scope
    /// </summary>
    public static OAuthJwtAuthenticationHandler CreateForScope(
        string tokenEndpoint,
        string clientId,
        string clientSecret,
        string scope,
        OAuthJwtHandlerOptions? options = null)
    {
        return CreateClientCredentialsHandler(tokenEndpoint, clientId, clientSecret, [scope], options);
    }

    /// <summary>
    /// Create OAuth JWT handler with method providers (for dynamic values)
    /// </summary>
    public static OAuthJwtAuthenticationHandler CreateWithProviders(
        string tokenEndpoint,
        Func<string> clientIdProvider,
        Func<string> clientSecretProvider,
        string scope,
        OAuthJwtHandlerOptions? options = null)
    {
        var oauthConfig = new OAuthClientConfiguration
        {
            TokenEndpoint = tokenEndpoint,
            ClientId = clientIdProvider(),
            ClientSecret = clientSecretProvider(),
            Scopes = { scope }
        };

        var tokenClient = new OAuthTokenClient();
        return new OAuthJwtAuthenticationHandler(tokenClient, oauthConfig, options);
    }

    /// <summary>
    /// Create OAuth JWT handler with dynamic configuration
    /// </summary>
    public static OAuthJwtAuthenticationHandler CreateWithConfiguration(
        Func<OAuthClientConfiguration> configurationProvider,
        OAuthJwtHandlerOptions? options = null)
    {
        var oauthConfig = configurationProvider();
        var tokenClient = new OAuthTokenClient();
        return new OAuthJwtAuthenticationHandler(tokenClient, oauthConfig, options);
    }

    /// <summary>
    /// Create OAuth JWT handler with additional parameters
    /// </summary>
    public static OAuthJwtAuthenticationHandler CreateWithParameters(
        string tokenEndpoint,
        string clientId,
        string clientSecret,
        string[]? scopes = null,
        Dictionary<string, string>? additionalParameters = null,
        Dictionary<string, string>? customHeaders = null,
        OAuthJwtHandlerOptions? options = null)
    {
        var oauthConfig = new OAuthClientConfiguration
        {
            TokenEndpoint = tokenEndpoint,
            ClientId = clientId,
            ClientSecret = clientSecret,
            Scopes = scopes?.ToList() ?? new List<string>(),
            AdditionalParameters = additionalParameters ?? new Dictionary<string, string>(),
            CustomHeaders = customHeaders ?? new Dictionary<string, string>()
        };

        var tokenClient = new OAuthTokenClient();
        return new OAuthJwtAuthenticationHandler(tokenClient, oauthConfig, options);
    }

    /// <summary>
    /// Create OAuth JWT handler with retry configuration
    /// </summary>
    public static OAuthJwtAuthenticationHandler CreateWithRetry(
        string tokenEndpoint,
        string clientId,
        string clientSecret,
        string[]? scopes = null,
        int maxRetries = 3,
        TimeSpan? initialDelay = null,
        OAuthJwtHandlerOptions? options = null)
    {
        var oauthConfig = new OAuthClientConfiguration
        {
            TokenEndpoint = tokenEndpoint,
            ClientId = clientId,
            ClientSecret = clientSecret,
            Scopes = scopes?.ToList() ?? [],
            RetryConfiguration = new OAuthRetryConfiguration
            {
                MaxRetries = maxRetries,
                InitialDelay = initialDelay ?? TimeSpan.FromSeconds(1)
            }
        };

        var tokenClient = new OAuthTokenClient();
        return new OAuthJwtAuthenticationHandler(tokenClient, oauthConfig, options);
    }
}
