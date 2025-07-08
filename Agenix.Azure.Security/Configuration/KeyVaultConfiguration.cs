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

using System.Security.Cryptography;
using System.Text;
using Azure.Core;

namespace Agenix.Azure.Security.Configuration;

/// <summary>
/// Configuration for Azure Key Vault SecretClient authentication
/// </summary>
public class KeyVaultConfiguration
{
    /// <summary>
    /// Azure Key Vault URL (e.g., https://myvault.vault.azure.net/)
    /// </summary>
    public string VaultUri { get; set; } = string.Empty;

    /// <summary>
    /// Azure AD tenant identifier
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Azure AD application (client) identifier
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Azure AD application secret
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// Azure AD authority URL (optional, defaults to public cloud)
    /// </summary>
    public string? Authority { get; set; }

    /// <summary>
    /// Maximum number of retry attempts
    /// </summary>
    public int? MaxRetries { get; set; }

    /// <summary>
    /// Delay between retry attempts
    /// </summary>
    public TimeSpan? RetryDelay { get; set; }

    /// <summary>
    /// Maximum delay between retry attempts
    /// </summary>
    public TimeSpan? MaxRetryDelay { get; set; }

    /// <summary>
    /// Retry mode (Fixed or Exponential)
    /// </summary>
    public RetryMode? RetryMode { get; set; }

    /// <summary>
    /// Enable logging for Azure SDK
    /// </summary>
    public bool? IsLoggingEnabled { get; set; }

    /// <summary>
    /// Enable content logging for Azure SDK
    /// </summary>
    public bool? IsLoggingContentEnabled { get; set; }

    /// <summary>
    /// Enable telemetry for Azure SDK
    /// </summary>
    public bool? IsTelemetryEnabled { get; set; }

    /// <summary>
    /// Application ID for telemetry
    /// </summary>
    public string? ApplicationId { get; set; }

    /// <summary>
    /// Request timeout for Key Vault operations
    /// </summary>
    public TimeSpan? RequestTimeout { get; set; }

    /// <summary>
    /// Additional client options for the SecretClient
    /// </summary>
    public Dictionary<string, object> AdditionalOptions { get; set; } = new();

    /// <summary>
    /// Get cache key for this configuration
    /// </summary>
    public string GetCacheKey()
    {
        var key = $"{VaultUri}|{TenantId}|{ClientId}|{Authority ?? "default"}";

        // Include additional options in cache key
        if (AdditionalOptions.Count > 0)
        {
            var additionalOpts = string.Join(",", AdditionalOptions.OrderBy(kv => kv.Key).Select(kv => $"{kv.Key}={kv.Value}"));
            key += $"|{additionalOpts}";
        }

        // Hash the key to keep it manageable
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        return Convert.ToBase64String(hashBytes).Replace('/', '_').Replace('+', '-').TrimEnd('=');
    }

    /// <summary>
    /// Validate configuration
    /// </summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(VaultUri))
            throw new InvalidOperationException("VaultUri is required");

        if (string.IsNullOrWhiteSpace(TenantId))
            throw new InvalidOperationException("TenantId is required");

        if (string.IsNullOrWhiteSpace(ClientId))
            throw new InvalidOperationException("ClientId is required");

        if (string.IsNullOrWhiteSpace(ClientSecret))
            throw new InvalidOperationException("ClientSecret is required");

        // Validate vault URI format
        if (!Uri.TryCreate(VaultUri, UriKind.Absolute, out var vaultUri))
            throw new InvalidOperationException("VaultUri must be a valid absolute URL");

        if (vaultUri.Scheme != "https")
            throw new InvalidOperationException("VaultUri must use HTTPS scheme");

        if (!vaultUri.Host.EndsWith(".vault.azure.net", StringComparison.OrdinalIgnoreCase) &&
            !vaultUri.Host.EndsWith(".vault.azure.cn", StringComparison.OrdinalIgnoreCase) &&
            !vaultUri.Host.EndsWith(".vault.usgovcloudapi.net", StringComparison.OrdinalIgnoreCase) &&
            !vaultUri.Host.EndsWith(".vault.microsoftazure.de", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("VaultUri must be a valid Azure Key Vault URL");

        // Validate tenant ID format (GUID or domain)
        if (!Guid.TryParse(TenantId, out _) && !IsValidDomainName(TenantId))
            throw new InvalidOperationException("TenantId must be a valid GUID or domain name");

        // Validate client ID format (GUID)
        if (!Guid.TryParse(ClientId, out _))
            throw new InvalidOperationException("ClientId must be a valid GUID");

        // Validate authority URL if provided
        if (!string.IsNullOrWhiteSpace(Authority))
        {
            if (!Uri.TryCreate(Authority, UriKind.Absolute, out var uri))
                throw new InvalidOperationException("Authority must be a valid absolute URL");

            if (uri.Scheme != "https")
                throw new InvalidOperationException("Authority must use HTTPS scheme");
        }
    }

    /// <summary>
    /// Check if a string is a valid domain name
    /// </summary>
    private static bool IsValidDomainName(string domain)
    {
        if (string.IsNullOrWhiteSpace(domain))
            return false;

        try
        {
            return Uri.CheckHostName(domain) != UriHostNameType.Unknown;
        }
        catch
        {
            return false;
        }
    }
}
