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

using System.Collections.Concurrent;
using Agenix.Api.Log;
using Agenix.Azure.Security.Configuration;
using Azure.Core;
using Azure.Core.Pipeline;
using Azure.Identity;
using Microsoft.Extensions.Logging;

namespace Agenix.Azure.Security.Client;

/// <summary>
/// Factory for creating and caching Azure AD ClientSecretCredential instances
/// </summary>
public static class ClientSecretCredentialFactory
{
    /// <summary>
    /// Logger instance.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(ClientSecretCredentialFactory));

    /// <summary>
    /// Cache for credential instances
    /// </summary>
    private static readonly ConcurrentDictionary<string, ClientSecretCredential> CredentialCache = new();

    /// <summary>
    /// Lock for thread-safe operations
    /// </summary>
    private static readonly SemaphoreSlim FactoryLock = new(1, 1);

    /// <summary>
    /// Get or retrieve a cached ClientSecretCredential instance
    /// </summary>
    /// <param name="configuration">Azure AD configuration</param>
    /// <returns>ClientSecretCredential instance</returns>
    public static async Task<ClientSecretCredential> GetCredentialAsync(AzureAdConfiguration configuration)
    {
        configuration.Validate();

        var cacheKey = configuration.GetCacheKey();

        if (CredentialCache.TryGetValue(cacheKey, out var existingCredential))
        {
            Log.LogDebug("Returning cached ClientSecretCredential for tenant: {TenantId}, client: {ClientId}",
                configuration.TenantId, configuration.ClientId);
            return existingCredential;
        }

        await FactoryLock.WaitAsync();
        try
        {
            // Double-check pattern
            if (CredentialCache.TryGetValue(cacheKey, out existingCredential))
            {
                return existingCredential;
            }

            Log.LogDebug("Creating new ClientSecretCredential for tenant: {TenantId}, client: {ClientId}",
                configuration.TenantId, configuration.ClientId);

            var options = CreateClientSecretCredentialOptions(configuration);
            var credential = new ClientSecretCredential(
                configuration.TenantId,
                configuration.ClientId,
                configuration.ClientSecret,
                options);

            CredentialCache.TryAdd(cacheKey, credential);

            Log.LogInformation("Successfully created ClientSecretCredential for tenant: {TenantId}, client: {ClientId}",
                configuration.TenantId, configuration.ClientId);

            return credential;
        }
        finally
        {
            FactoryLock.Release();
        }
    }

    /// <summary>
    /// Get a ClientSecretCredential instance synchronously
    /// </summary>
    /// <param name="configuration">Azure AD configuration</param>
    /// <returns>ClientSecretCredential instance</returns>
    public static ClientSecretCredential GetCredential(AzureAdConfiguration configuration)
    {
        return GetCredentialAsync(configuration).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Get a ClientSecretCredential with individual parameters
    /// </summary>
    /// <param name="tenantId">Azure AD tenant identifier</param>
    /// <param name="clientId">Azure AD application identifier</param>
    /// <param name="clientSecret">Azure AD application secret</param>
    /// <param name="authority">Optional authority URL</param>
    /// <returns>ClientSecretCredential instance</returns>
    public static ClientSecretCredential GetCredential(string tenantId, string clientId, string clientSecret, string? authority = null)
    {
        var configuration = new AzureAdConfiguration
        {
            TenantId = tenantId,
            ClientId = clientId,
            ClientSecret = clientSecret,
            Authority = authority
        };

        return GetCredential(configuration);
    }

    /// <summary>
    /// Create ClientSecretCredentialOptions from configuration
    /// </summary>
    /// <param name="configuration">Azure AD configuration</param>
    /// <returns>ClientSecretCredentialOptions</returns>
    private static ClientSecretCredentialOptions CreateClientSecretCredentialOptions(AzureAdConfiguration configuration)
    {
        var options = new ClientSecretCredentialOptions();

        if (!string.IsNullOrWhiteSpace(configuration.Authority))
        {
            options.AuthorityHost = new Uri(configuration.Authority);
        }

        // Configure retry options
        if (configuration.MaxRetries.HasValue)
        {
            options.Retry.MaxRetries = configuration.MaxRetries.Value;
        }

        if (configuration.RetryDelay.HasValue)
        {
            options.Retry.Delay = configuration.RetryDelay.Value;
        }

        if (configuration.MaxRetryDelay.HasValue)
        {
            options.Retry.MaxDelay = configuration.MaxRetryDelay.Value;
        }

        if (configuration.RetryMode.HasValue)
        {
            options.Retry.Mode = configuration.RetryMode.Value;
        }

        // Configure diagnostics options
        if (configuration.IsLoggingEnabled.HasValue)
        {
            options.Diagnostics.IsLoggingEnabled = configuration.IsLoggingEnabled.Value;
        }

        if (configuration.IsLoggingContentEnabled.HasValue)
        {
            options.Diagnostics.IsLoggingContentEnabled = configuration.IsLoggingContentEnabled.Value;
        }

        if (configuration.IsTelemetryEnabled.HasValue)
        {
            options.Diagnostics.IsTelemetryEnabled = configuration.IsTelemetryEnabled.Value;
        }

        if (!string.IsNullOrWhiteSpace(configuration.ApplicationId))
        {
            options.Diagnostics.ApplicationId = configuration.ApplicationId;
        }

        // Apply additional options if any
        foreach (var option in configuration.AdditionalOptions)
        {
            ApplyAdditionalOption(options, option.Key, option.Value);
        }

        return options;
    }


    /// <summary>
    /// Apply additional configuration options
    /// </summary>
    /// <param name="options">ClientSecretCredentialOptions to modify</param>
    /// <param name="key">Option key</param>
    /// <param name="value">Option value</param>
    private static void ApplyAdditionalOption(ClientSecretCredentialOptions options, string key, object value)
    {
        switch (key.ToLowerInvariant())
        {
            case "maxretries":
                if (value is int maxRetries)
                {
                    options.Retry.MaxRetries = maxRetries;
                }
                break;
            case "retrydelay":
                if (value is TimeSpan delay)
                {
                    options.Retry.Delay = delay;
                }
                break;
            case "maxretrydelay":
                if (value is TimeSpan maxDelay)
                {
                    options.Retry.MaxDelay = maxDelay;
                }
                break;
            case "retrymode":
                if (value is RetryMode retryMode)
                {
                    options.Retry.Mode = retryMode;
                }
                break;
            case "transport":
                if (value is HttpPipelineTransport transport)
                {
                    options.Transport = transport;
                }
                break;
            case "isloggingcontentallowed":
                if (value is bool isLoggingContentAllowed)
                {
                    options.Diagnostics.IsLoggingContentEnabled = isLoggingContentAllowed;
                }
                break;
            case "isloggingenabled":
                if (value is bool isLoggingEnabled)
                {
                    options.Diagnostics.IsLoggingEnabled = isLoggingEnabled;
                }
                break;
            case "istelemetryenabled":
                if (value is bool isTelemetryEnabled)
                {
                    options.Diagnostics.IsTelemetryEnabled = isTelemetryEnabled;
                }
                break;
            case "applicationid":
                if (value is string applicationId)
                {
                    options.Diagnostics.ApplicationId = applicationId;
                }
                break;
            default:
                Log.LogWarning("Unknown ClientSecretCredentialOptions property: {Key}", key);
                break;
        }
    }

    /// <summary>
    /// Clear the credential cache
    /// </summary>
    public static async Task ClearCacheAsync()
    {
        await FactoryLock.WaitAsync();
        try
        {
            Log.LogDebug("Clearing ClientSecretCredential cache");
            CredentialCache.Clear();
            Log.LogInformation("ClientSecretCredential cache cleared");
        }
        finally
        {
            FactoryLock.Release();
        }
    }

    /// <summary>
    /// Clear the credential cache synchronously
    /// </summary>
    public static void ClearCache()
    {
        ClearCacheAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Get the number of cached credentials
    /// </summary>
    public static int CachedCredentialCount => CredentialCache.Count;

    /// <summary>
    /// Remove a specific credential from cache
    /// </summary>
    /// <param name="configuration">Configuration of the credential to remove</param>
    /// <returns>True if the credential was found and removed, false otherwise</returns>
    public static async Task<bool> RemoveCredentialAsync(AzureAdConfiguration configuration)
    {
        var cacheKey = configuration.GetCacheKey();

        await FactoryLock.WaitAsync();
        try
        {
            var removed = CredentialCache.TryRemove(cacheKey, out _);
            if (removed)
            {
                Log.LogDebug("Removed ClientSecretCredential from cache for tenant: {TenantId}, client: {ClientId}",
                    configuration.TenantId, configuration.ClientId);
            }
            return removed;
        }
        finally
        {
            FactoryLock.Release();
        }
    }

    /// <summary>
    /// Remove a specific credential from cache synchronously
    /// </summary>
    /// <param name="configuration">Configuration of the credential to remove</param>
    /// <returns>True if the credential was found and removed, false otherwise</returns>
    public static bool RemoveCredential(AzureAdConfiguration configuration)
    {
        return RemoveCredentialAsync(configuration).GetAwaiter().GetResult();
    }
}
