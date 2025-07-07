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
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;

namespace Agenix.Azure.Security.Client;

/// <summary>
///     Factory for creating and caching Azure Key Vault SecretClient instances
/// </summary>
public static class SecretClientFactory
{
    /// <summary>
    ///     Logger instance.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(SecretClientFactory));

    /// <summary>
    ///     Cache for SecretClient instances
    /// </summary>
    private static readonly ConcurrentDictionary<string, SecretClient> ClientCache = new();

    /// <summary>
    ///     Lock for thread-safe operations
    /// </summary>
    private static readonly SemaphoreSlim FactoryLock = new(1, 1);

    /// <summary>
    ///     Get the number of cached clients
    /// </summary>
    public static int CachedClientCount => ClientCache.Count;

    /// <summary>
    ///     Get or retrieve a cached SecretClient instance
    /// </summary>
    /// <param name="configuration">Key Vault configuration</param>
    /// <returns>SecretClient instance</returns>
    public static async Task<SecretClient> GetClientAsync(KeyVaultConfiguration configuration)
    {
        configuration.Validate();

        var cacheKey = configuration.GetCacheKey();

        if (ClientCache.TryGetValue(cacheKey, out var existingClient))
        {
            Log.LogDebug("Returning cached SecretClient for vault: {VaultUri}, tenant: {TenantId}",
                configuration.VaultUri, configuration.TenantId);
            return existingClient;
        }

        await FactoryLock.WaitAsync();
        try
        {
            // Double-check pattern
            if (ClientCache.TryGetValue(cacheKey, out existingClient))
            {
                return existingClient;
            }

            Log.LogDebug("Creating new SecretClient for vault: {VaultUri}, tenant: {TenantId}",
                configuration.VaultUri, configuration.TenantId);

            var credential = CreateCredential(configuration);
            var options = CreateSecretClientOptions(configuration);
            var client = new SecretClient(new Uri(configuration.VaultUri), credential, options);

            ClientCache.TryAdd(cacheKey, client);

            Log.LogInformation("Successfully created SecretClient for vault: {VaultUri}, tenant: {TenantId}",
                configuration.VaultUri, configuration.TenantId);

            return client;
        }
        finally
        {
            FactoryLock.Release();
        }
    }

    /// <summary>
    ///     Get a SecretClient instance synchronously
    /// </summary>
    /// <param name="configuration">Key Vault configuration</param>
    /// <returns>SecretClient instance</returns>
    public static SecretClient GetClient(KeyVaultConfiguration configuration)
    {
        return GetClientAsync(configuration).GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Get a SecretClient with individual parameters
    /// </summary>
    /// <param name="vaultUri">Azure Key Vault URI</param>
    /// <param name="tenantId">Azure AD tenant identifier</param>
    /// <param name="clientId">Azure AD application identifier</param>
    /// <param name="clientSecret">Azure AD application secret</param>
    /// <param name="authority">Optional authority URL</param>
    /// <returns>SecretClient instance</returns>
    public static SecretClient GetClient(string vaultUri, string tenantId, string clientId, string clientSecret,
        string? authority = null)
    {
        var configuration = new KeyVaultConfiguration
        {
            VaultUri = vaultUri,
            TenantId = tenantId,
            ClientId = clientId,
            ClientSecret = clientSecret,
            Authority = authority
        };

        return GetClient(configuration);
    }

    /// <summary>
    ///     Create ClientSecretCredential from configuration
    /// </summary>
    /// <param name="configuration">Key Vault configuration</param>
    /// <returns>ClientSecretCredential instance</returns>
    private static ClientSecretCredential CreateCredential(KeyVaultConfiguration configuration)
    {
        var credentialOptions = new ClientSecretCredentialOptions();

        if (!string.IsNullOrWhiteSpace(configuration.Authority))
        {
            credentialOptions.AuthorityHost = new Uri(configuration.Authority);
        }

        // Configure retry options for credential
        if (configuration.MaxRetries.HasValue)
        {
            credentialOptions.Retry.MaxRetries = configuration.MaxRetries.Value;
        }

        if (configuration.RetryDelay.HasValue)
        {
            credentialOptions.Retry.Delay = configuration.RetryDelay.Value;
        }

        if (configuration.MaxRetryDelay.HasValue)
        {
            credentialOptions.Retry.MaxDelay = configuration.MaxRetryDelay.Value;
        }

        if (configuration.RetryMode.HasValue)
        {
            credentialOptions.Retry.Mode = configuration.RetryMode.Value;
        }

        return new ClientSecretCredential(
            configuration.TenantId,
            configuration.ClientId,
            configuration.ClientSecret,
            credentialOptions);
    }

    /// <summary>
    ///     Create SecretClientOptions from configuration
    /// </summary>
    /// <param name="configuration">Key Vault configuration</param>
    /// <returns>SecretClientOptions</returns>
    private static SecretClientOptions CreateSecretClientOptions(KeyVaultConfiguration configuration)
    {
        var options = new SecretClientOptions();

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
    ///     Apply additional configuration options
    /// </summary>
    /// <param name="options">SecretClientOptions to modify</param>
    /// <param name="key">Option key</param>
    /// <param name="value">Option value</param>
    private static void ApplyAdditionalOption(SecretClientOptions options, string key, object value)
    {
        switch (key.ToLowerInvariant())
        {
            case "maxretries":
            case "retrydelay":
            case "maxretrydelay":
            case "retrymode":
                ApplyRetryOption(options, key.ToLowerInvariant(), value);
                break;
            case "transport":
                if (value is HttpPipelineTransport transport)
                {
                    options.Transport = transport;
                }

                break;
            case "isloggingcontentallowed":
            case "isloggingenabled":
            case "istelemetryenabled":
            case "applicationid":
                ApplyDiagnosticsOption(options, key.ToLowerInvariant(), value);
                break;
            default:
                Log.LogWarning("Unknown SecretClientOptions property: {Key}", key);
                break;
        }
    }

    private static void ApplyRetryOption(SecretClientOptions options, string key, object value)
    {
        switch (key)
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
        }
    }

    private static void ApplyDiagnosticsOption(SecretClientOptions options, string key, object value)
    {
        switch (key)
        {
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
        }
    }

    /// <summary>
    ///     Clear the client cache
    /// </summary>
    public static async Task ClearCacheAsync()
    {
        await FactoryLock.WaitAsync();
        try
        {
            Log.LogDebug("Clearing SecretClient cache");
            ClientCache.Clear();
            Log.LogInformation("SecretClient cache cleared");
        }
        finally
        {
            FactoryLock.Release();
        }
    }

    /// <summary>
    ///     Clear the client cache synchronously
    /// </summary>
    public static void ClearCache()
    {
        ClearCacheAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    ///     Remove a specific client from cache
    /// </summary>
    /// <param name="configuration">Configuration of the client to remove</param>
    /// <returns>True if the client was found and removed, false otherwise</returns>
    public static async Task<bool> RemoveClientAsync(KeyVaultConfiguration configuration)
    {
        var cacheKey = configuration.GetCacheKey();

        await FactoryLock.WaitAsync();
        try
        {
            var removed = ClientCache.TryRemove(cacheKey, out _);
            if (removed)
            {
                Log.LogDebug("Removed SecretClient from cache for vault: {VaultUri}, tenant: {TenantId}",
                    configuration.VaultUri, configuration.TenantId);
            }

            return removed;
        }
        finally
        {
            FactoryLock.Release();
        }
    }

    /// <summary>
    ///     Remove a specific client from cache synchronously
    /// </summary>
    /// <param name="configuration">Configuration of the client to remove</param>
    /// <returns>True if the client was found and removed, false otherwise</returns>
    public static bool RemoveClient(KeyVaultConfiguration configuration)
    {
        return RemoveClientAsync(configuration).GetAwaiter().GetResult();
    }
}
