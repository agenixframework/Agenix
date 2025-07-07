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

using System.Text.Json;
using Agenix.Api.Log;
using Agenix.Azure.Security.Configuration;
using Microsoft.Extensions.Logging;

namespace Agenix.Azure.Security.Client;

/// <summary>
///     Factory for creating multi-vault secret clients with various configurations
/// </summary>
public static class MultiVaultClientFactory
{
    /// <summary>
    ///     Logger instance
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(MultiVaultClientFactory));

    /// <summary>
    ///     Create MultiVaultSecretClient with multiple vault URIs using same credentials
    /// </summary>
    /// <param name="vaultUris">Array of Key Vault URIs</param>
    /// <param name="tenantId">Azure AD tenant ID</param>
    /// <param name="clientId">Azure AD client ID</param>
    /// <param name="clientSecret">Azure AD client secret</param>
    /// <param name="strategy">Search strategy to use</param>
    /// <param name="authority">Azure AD authority (optional)</param>
    /// <returns>Configured MultiVaultSecretClient</returns>
    public static MultiVaultSecretClient CreateClient(
        string[] vaultUris,
        string tenantId,
        string clientId,
        string clientSecret,
        SecretSearchStrategy strategy = SecretSearchStrategy.FIRST_FOUND,
        string? authority = null)
    {
        if (vaultUris == null || vaultUris.Length == 0)
        {
            throw new ArgumentException("At least one vault URI is required", nameof(vaultUris));
        }

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentException("Tenant ID is required", nameof(tenantId));
        }

        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new ArgumentException("Client ID is required", nameof(clientId));
        }

        if (string.IsNullOrWhiteSpace(clientSecret))
        {
            throw new ArgumentException("Client secret is required", nameof(clientSecret));
        }

        Log.LogDebug("Creating MultiVaultSecretClient with {VaultCount} vaults using shared credentials",
            vaultUris.Length);

        var configurations = vaultUris.Select(uri => new KeyVaultConfiguration
        {
            VaultUri = uri.Trim(),
            TenantId = tenantId,
            ClientId = clientId,
            ClientSecret = clientSecret,
            Authority = authority
        }).ToList();

        // Validate all URIs
        foreach (var config in configurations)
        {
            config.Validate();
        }

        return new MultiVaultSecretClient(configurations, strategy);
    }

    /// <summary>
    ///     Create MultiVaultSecretClient with different configurations per vault
    /// </summary>
    /// <param name="configurations">Array of Key Vault configurations</param>
    /// <param name="strategy">Search strategy to use</param>
    /// <returns>Configured MultiVaultSecretClient</returns>
    public static MultiVaultSecretClient CreateClient(
        KeyVaultConfiguration[] configurations,
        SecretSearchStrategy strategy = SecretSearchStrategy.FIRST_FOUND)
    {
        if (configurations == null || configurations.Length == 0)
        {
            throw new ArgumentException("At least one configuration is required", nameof(configurations));
        }

        Log.LogDebug("Creating MultiVaultSecretClient with {VaultCount} vaults using individual configurations",
            configurations.Length);

        // Validate all configurations
        foreach (var config in configurations)
        {
            config.Validate();
        }

        return new MultiVaultSecretClient(configurations, strategy);
    }

    /// <summary>
    ///     Create MultiVaultSecretClient from environment-based configuration
    /// </summary>
    /// <param name="environmentPrefix">Environment variable prefix (e.g., "KEYVAULT_")</param>
    /// <param name="strategy">Search strategy to use</param>
    /// <returns>Configured MultiVaultSecretClient</returns>
    public static MultiVaultSecretClient CreateFromEnvironment(
        string environmentPrefix = "KEYVAULT_",
        SecretSearchStrategy strategy = SecretSearchStrategy.FIRST_FOUND)
    {
        if (string.IsNullOrWhiteSpace(environmentPrefix))
        {
            throw new ArgumentException("Environment prefix cannot be null or empty", nameof(environmentPrefix));
        }

        Log.LogDebug("Creating MultiVaultSecretClient from environment variables with prefix '{Prefix}'",
            environmentPrefix);

        // Read and validate credentials
        var (tenantId, clientId, clientSecret, authority) = ReadAndValidateCredentials(environmentPrefix);

        // Read vault URIs
        var vaultUris = ReadVaultUris(environmentPrefix);

        Log.LogInformation("Found {VaultCount} vault URIs from environment variables", vaultUris.Count);

        return CreateClient(vaultUris.ToArray(), tenantId, clientId, clientSecret, strategy, authority);
    }

    private static (string tenantId, string clientId, string clientSecret, string authority) ReadAndValidateCredentials(
        string environmentPrefix)
    {
        var tenantId = Environment.GetEnvironmentVariable($"{environmentPrefix}TENANT_ID");
        var clientId = Environment.GetEnvironmentVariable($"{environmentPrefix}CLIENT_ID");
        var clientSecret = Environment.GetEnvironmentVariable($"{environmentPrefix}CLIENT_SECRET");
        var authority = Environment.GetEnvironmentVariable($"{environmentPrefix}AUTHORITY");

        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new InvalidOperationException($"Environment variable '{environmentPrefix}TENANT_ID' is required");
        }

        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new InvalidOperationException($"Environment variable '{environmentPrefix}CLIENT_ID' is required");
        }

        if (string.IsNullOrWhiteSpace(clientSecret))
        {
            throw new InvalidOperationException($"Environment variable '{environmentPrefix}CLIENT_SECRET' is required");
        }

        return (tenantId, clientId, clientSecret, authority);
    }

    private static List<string> ReadVaultUris(string environmentPrefix)
    {
        var vaultUris = new List<string>();

        // Try comma-separated first
        var urisString = Environment.GetEnvironmentVariable($"{environmentPrefix}URIS");
        if (!string.IsNullOrWhiteSpace(urisString))
        {
            vaultUris.AddRange(urisString.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(uri => uri.Trim()));
        }
        else
        {
            ReadNumberedVaultUris(environmentPrefix, vaultUris);
        }

        if (vaultUris.Count == 0)
        {
            throw new InvalidOperationException(
                $"No vault URIs found. Set either '{environmentPrefix}URIS' (comma-separated) or '{environmentPrefix}URI_1', '{environmentPrefix}URI_2', etc.");
        }

        return vaultUris;
    }

    private static void ReadNumberedVaultUris(string environmentPrefix, List<string> vaultUris)
    {
        for (var i = 1; i <= 10; i++) // Check up to 10 vaults
        {
            var uri = Environment.GetEnvironmentVariable($"{environmentPrefix}URI_{i}");
            if (!string.IsNullOrWhiteSpace(uri))
            {
                vaultUris.Add(uri.Trim());
            }
            else if (i == 1)
            {
                // If URI_1 doesn't exist, try just URI
                uri = Environment.GetEnvironmentVariable($"{environmentPrefix}URI");
                if (!string.IsNullOrWhiteSpace(uri))
                {
                    vaultUris.Add(uri.Trim());
                }

                break;
            }
        }
    }

    /// <summary>
    ///     Create MultiVaultSecretClient for specific environments (prod, staging, dev)
    /// </summary>
    /// <param name="environmentTier">Environment tier (prod, staging, dev, etc.)</param>
    /// <param name="tenantId">Azure AD tenant ID</param>
    /// <param name="clientId">Azure AD client ID</param>
    /// <param name="clientSecret">Azure AD client secret</param>
    /// <param name="vaultNamePattern">Vault name pattern with {env} placeholder</param>
    /// <param name="strategy">Search strategy to use</param>
    /// <param name="authority">Azure AD authority (optional)</param>
    /// <returns>Configured MultiVaultSecretClient</returns>
    public static MultiVaultSecretClient CreateForEnvironment(
        string environmentTier,
        string tenantId,
        string clientId,
        string clientSecret,
        string vaultNamePattern = "{env}-vault",
        SecretSearchStrategy strategy = SecretSearchStrategy.PRIORITY_ORDER,
        string? authority = null)
    {
        if (string.IsNullOrWhiteSpace(environmentTier))
        {
            throw new ArgumentException("Environment tier is required", nameof(environmentTier));
        }

        if (string.IsNullOrWhiteSpace(vaultNamePattern))
        {
            throw new ArgumentException("Vault name pattern is required", nameof(vaultNamePattern));
        }

        Log.LogDebug(
            "Creating MultiVaultSecretClient for environment tier '{EnvironmentTier}' with pattern '{Pattern}'",
            environmentTier, vaultNamePattern);

        var environments = environmentTier.ToLowerInvariant() switch
        {
            "prod" or "production" => new[] { "prod" },
            "staging" or "stage" => new[] { "staging", "prod" },
            "dev" or "development" => new[] { "dev", "staging" },
            "test" or "testing" => new[] { "test", "dev" },
            _ => new[] { environmentTier.ToLowerInvariant(), "dev" }
        };

        var vaultUris = environments.Select(env =>
        {
            var vaultName = vaultNamePattern.Replace("{env}", env);
            return $"https://{vaultName}.vault.azure.net/";
        }).ToArray();

        Log.LogInformation("Created vault URIs for environment '{EnvironmentTier}': {VaultUris}",
            environmentTier, string.Join(", ", vaultUris));

        return CreateClient(vaultUris, tenantId, clientId, clientSecret, strategy, authority);
    }


    /// <summary>
    /// Options used for configuring the creation of a multi-vault secret client.
    /// </summary>
    public class MultiVaultClientOptions
    {
        /// <summary>
        /// A collection of URIs representing the Azure Key Vault instances to be used in the multi-vault configuration.
        /// </summary>
        public string[] VaultUris { get; set; } = [];

        /// <summary>
        /// Gets or sets the tenant identifier associated with the Azure Active Directory.
        /// </summary>
        public string TenantId { get; set; } = string.Empty;

        /// <summary>
        /// Identifier for the client application interacting with Azure Key Vault.
        /// </summary>
        public string ClientId { get; set; } = string.Empty;

        /// <summary>
        /// The client secret used for authentication with Azure Key Vault.
        /// </summary>
        public string ClientSecret { get; set; } = string.Empty;

        /// <summary>
        /// The maximum number of retry attempts for failed operations.
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// The optional delay applied between retry attempts when accessing the vault.
        /// </summary>
        public TimeSpan? RetryDelay { get; set; }

        /// <summary>
        /// Defines the strategy to use when searching for secrets across multiple vaults.
        /// </summary>
        public SecretSearchStrategy Strategy { get; set; } = SecretSearchStrategy.FIRST_FOUND;

        /// <summary>
        /// Gets or sets the Azure Active Directory authority URL used for authentication.
        /// This property specifies the endpoint that will be used to acquire tokens
        /// during the authentication process.
        /// </summary>
        public string? Authority { get; set; }
    }

    /// <summary>
    /// Create MultiVaultSecretClient with custom retry configuration
    /// </summary>
    /// <param name="options">Options containing configuration details such as vault URIs, tenant ID, client ID, client secret, retry settings, and authority</param>
    /// <returns>Configured MultiVaultSecretClient</returns>
    public static MultiVaultSecretClient CreateWithRetry(MultiVaultClientOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (options.VaultUris == null || options.VaultUris.Length == 0)
        {
            throw new ArgumentException("At least one vault URI is required", nameof(options));
        }

        var retryDelay = options.RetryDelay ?? TimeSpan.FromSeconds(1);

        Log.LogDebug(
            "Creating MultiVaultSecretClient with retry configuration: {MaxRetries} retries, {RetryDelay} delay",
            options.MaxRetries, retryDelay);

        var configurations = options.VaultUris.Select(uri => new KeyVaultConfiguration
        {
            VaultUri = uri.Trim(),
            TenantId = options.TenantId,
            ClientId = options.ClientId,
            ClientSecret = options.ClientSecret,
            Authority = options.Authority,
            MaxRetries = options.MaxRetries,
            RetryDelay = retryDelay
        }).ToList();

        // Validate all configurations
        foreach (var config in configurations)
        {
            config.Validate();
        }

        return new MultiVaultSecretClient(configurations, options.Strategy);
    }

    /// <summary>
    ///     Create MultiVaultSecretClient from configuration file (JSON/XML)
    /// </summary>
    /// <param name="configurationFilePath">Path to configuration file</param>
    /// <param name="strategy">Search strategy to use</param>
    /// <returns>Configured MultiVaultSecretClient</returns>
    public static MultiVaultSecretClient CreateFromConfigFile(
        string configurationFilePath,
        SecretSearchStrategy strategy = SecretSearchStrategy.FIRST_FOUND)
    {
        if (string.IsNullOrWhiteSpace(configurationFilePath))
        {
            throw new ArgumentException("Configuration file path is required", nameof(configurationFilePath));
        }

        if (!File.Exists(configurationFilePath))
        {
            throw new FileNotFoundException($"Configuration file not found: {configurationFilePath}");
        }

        Log.LogDebug("Creating MultiVaultSecretClient from configuration file '{FilePath}'", configurationFilePath);

        try
        {
            var json = File.ReadAllText(configurationFilePath);
            var configData = JsonSerializer.Deserialize<MultiVaultConfig>(json);

            if (configData?.Vaults == null || !configData.Vaults.Any())
            {
                throw new InvalidOperationException("Configuration file must contain at least one vault configuration");
            }

            var configurations = configData.Vaults.Select(v => new KeyVaultConfiguration
            {
                VaultUri = v.VaultUri,
                TenantId = v.TenantId,
                ClientId = v.ClientId,
                ClientSecret = v.ClientSecret,
                Authority = v.Authority,
                MaxRetries = v.MaxRetries,
                RetryDelay = v.RetryDelay.HasValue ? TimeSpan.FromMilliseconds(v.RetryDelay.Value) : null
            }).ToArray();

            Log.LogInformation("Loaded {VaultCount} vault configurations from file", configurations.Length);

            return CreateClient(configurations, strategy);
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Failed to load configuration from file '{FilePath}'", configurationFilePath);
            throw new InvalidOperationException($"Failed to load configuration from file: {ex.Message}", ex);
        }
    }

    /// <summary>
    ///     Create a simple MultiVaultSecretClient for testing/development
    /// </summary>
    /// <param name="primaryVaultUri">Primary vault URI</param>
    /// <param name="fallbackVaultUri">Fallback vault URI</param>
    /// <param name="tenantId">Azure AD tenant ID</param>
    /// <param name="clientId">Azure AD client ID</param>
    /// <param name="clientSecret">Azure AD client secret</param>
    /// <returns>Configured MultiVaultSecretClient with FirstFound strategy</returns>
    public static MultiVaultSecretClient CreateSimple(
        string primaryVaultUri,
        string fallbackVaultUri,
        string tenantId,
        string clientId,
        string clientSecret)
    {
        Log.LogDebug("Creating simple MultiVaultSecretClient with primary '{Primary}' and fallback '{Fallback}'",
            primaryVaultUri, fallbackVaultUri);

        return CreateClient(
            [primaryVaultUri, fallbackVaultUri],
            tenantId,
            clientId,
            clientSecret
        );
    }
}

/// <summary>
///     Configuration structure for JSON file loading
/// </summary>
internal class MultiVaultConfig
{
    public VaultConfig[] Vaults { get; set; } = [];
}

/// <summary>
///     Individual vault configuration for JSON file loading
/// </summary>
internal class VaultConfig
{
    public string VaultUri { get; set; } = string.Empty;
    public string TenantId { get; set; } = string.Empty;
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string? Authority { get; set; }
    public int? MaxRetries { get; set; }
    public int? RetryDelay { get; set; } // in milliseconds
}
