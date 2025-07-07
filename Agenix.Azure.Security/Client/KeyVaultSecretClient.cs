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

using Agenix.Api.Log;
using Agenix.Azure.Security.Configuration;
using Azure;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;

namespace Agenix.Azure.Security.Client;

/// <summary>
/// Client for retrieving secrets from Azure Key Vault
/// </summary>
public class KeyVaultSecretClient
{
    /// <summary>
    /// Logger instance
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(KeyVaultSecretClient));

    /// <summary>
    /// The underlying Azure SecretClient
    /// </summary>
    private readonly SecretClient _secretClient;

    /// <summary>
    /// Key Vault configuration
    /// </summary>
    private readonly KeyVaultConfiguration _configuration;

    /// <summary>
    /// Initialize KeyVaultSecretClient with configuration
    /// </summary>
    /// <param name="configuration">Key Vault configuration</param>
    public KeyVaultSecretClient(KeyVaultConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _secretClient = SecretClientFactory.GetClient(configuration);
    }

    /// <summary>
    /// Initialize KeyVaultSecretClient with existing SecretClient
    /// </summary>
    /// <param name="secretClient">Azure SecretClient instance</param>
    /// <param name="configuration">Key Vault configuration</param>
    public KeyVaultSecretClient(SecretClient secretClient, KeyVaultConfiguration configuration)
    {
        _secretClient = secretClient ?? throw new ArgumentNullException(nameof(secretClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    /// <summary>
    /// Get secret value as string
    /// </summary>
    /// <param name="secretName">Name of the secret</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Secret value as string</returns>
    public async Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(secretName))
            throw new ArgumentException("Secret name cannot be null or empty", nameof(secretName));

        try
        {
            Log.LogDebug("Retrieving secret '{SecretName}' from vault '{VaultUri}'", secretName, _configuration.VaultUri);

            var response = await _secretClient.GetSecretAsync(secretName, cancellationToken: cancellationToken);
            var secretValue = response.Value.Value;

            Log.LogDebug("Successfully retrieved secret '{SecretName}' from vault '{VaultUri}'", secretName, _configuration.VaultUri);
            return secretValue;
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Failed to retrieve secret '{SecretName}' from vault '{VaultUri}'", secretName, _configuration.VaultUri);
            throw new InvalidOperationException($"Failed to retrieve secret '{secretName}' from vault '{_configuration.VaultUri}'", ex);
        }
    }

    /// <summary>
    /// Get specific version of secret value as string
    /// </summary>
    /// <param name="secretName">Name of the secret</param>
    /// <param name="version">Secret version</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Secret value as string</returns>
    public async Task<string> GetSecretAsync(string secretName, string version, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(secretName))
            throw new ArgumentException("Secret name cannot be null or empty", nameof(secretName));

        if (string.IsNullOrWhiteSpace(version))
            throw new ArgumentException("Secret version cannot be null or empty", nameof(version));

        try
        {
            Log.LogDebug("Retrieving secret '{SecretName}' version '{Version}' from vault '{VaultUri}'",
                secretName, version, _configuration.VaultUri);

            var response = await _secretClient.GetSecretAsync(secretName, version, cancellationToken);
            var secretValue = response.Value.Value;

            Log.LogDebug("Successfully retrieved secret '{SecretName}' version '{Version}' from vault '{VaultUri}'",
                secretName, version, _configuration.VaultUri);
            return secretValue;
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Failed to retrieve secret '{SecretName}' version '{Version}' from vault '{VaultUri}'",
                secretName, version, _configuration.VaultUri);
            throw new InvalidOperationException($"Failed to retrieve secret '{secretName}' from vault '{_configuration.VaultUri}'", ex);
        }
    }

    /// <summary>
    /// Get multiple secrets at once
    /// </summary>
    /// <param name="secretNames">Names of the secrets to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary with secret names as keys and secret values as values</returns>
    public async Task<Dictionary<string, string>> GetSecretsAsync(IEnumerable<string> secretNames, CancellationToken cancellationToken = default)
    {
        if (secretNames == null)
            throw new ArgumentNullException(nameof(secretNames));

        var secretNamesList = secretNames.ToList();
        if (secretNamesList.Count == 0)
            return new Dictionary<string, string>();

        Log.LogDebug("Retrieving {Count} secrets from vault '{VaultUri}'", secretNamesList.Count, _configuration.VaultUri);

        var results = new Dictionary<string, string>();
        var tasks = secretNamesList.Select(async secretName =>
        {
            try
            {
                var value = await GetSecretAsync(secretName, cancellationToken);
                return new KeyValuePair<string, string>(secretName, value);
            }
            catch (Exception ex)
            {
                Log.LogWarning(ex, "Failed to retrieve secret '{SecretName}' while getting multiple secrets", secretName);
                return new KeyValuePair<string, string>(secretName, string.Empty);
            }
        });

        var completedTasks = await Task.WhenAll(tasks);

        foreach (var result in completedTasks)
        {
            if (!string.IsNullOrEmpty(result.Value))
            {
                results[result.Key] = result.Value;
            }
        }

        Log.LogDebug("Successfully retrieved {Count}/{Total} secrets from vault '{VaultUri}'",
            results.Count, secretNamesList.Count, _configuration.VaultUri);

        return results;
    }

    /// <summary>
    /// Get a specific version of secret value as string synchronously
    /// </summary>
    /// <param name="secretName">Name of the secret</param>
    /// <param name="version">Secret version</param>
    /// <returns>Secret value as string</returns>
    public string GetSecret(string secretName, string version)
    {
        return GetSecretAsync(secretName, version).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Get secret value as string synchronously
    /// </summary>
    /// <param name="secretName">Name of the secret</param>
    /// <returns>Secret value as string</returns>
    public string GetSecret(string secretName)
    {
        return GetSecretAsync(secretName).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Get multiple secrets at once synchronously
    /// </summary>
    /// <param name="secretNames">Names of the secrets to retrieve</param>
    /// <returns>Dictionary with secret names as keys and secret values as values</returns>
    public Dictionary<string, string> GetSecrets(IEnumerable<string> secretNames)
    {
        return GetSecretsAsync(secretNames).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Check if a secret exists
    /// </summary>
    /// <param name="secretName">Name of the secret</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if a secret exists, false otherwise</returns>
    public async Task<bool> SecretExistsAsync(string secretName, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(secretName))
            throw new ArgumentException("Secret name cannot be null or empty", nameof(secretName));

        try
        {
            Log.LogDebug("Checking if secret '{SecretName}' exists in vault '{VaultUri}'", secretName, _configuration.VaultUri);

            await _secretClient.GetSecretAsync(secretName, cancellationToken: cancellationToken);

            Log.LogDebug("Secret '{SecretName}' exists in vault '{VaultUri}'", secretName, _configuration.VaultUri);
            return true;
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            Log.LogDebug(ex, "Secret '{SecretName}' does not exist in vault '{VaultUri}'", secretName, _configuration.VaultUri);
            return false;
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Error checking if secret '{SecretName}' exists in vault '{VaultUri}'", secretName, _configuration.VaultUri);
            throw new InvalidOperationException($"Failed to check if secret '{secretName}' exists in vault '{_configuration.VaultUri}'", ex);
        }
    }

    /// <summary>
    /// Check if a secret exists synchronously
    /// </summary>
    /// <param name="secretName">Name of the secret</param>
    /// <returns>True if a secret exists, false otherwise</returns>
    public bool SecretExists(string secretName)
    {
        return SecretExistsAsync(secretName).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Get the vault URI
    /// </summary>
    public string VaultUri => _configuration.VaultUri;

    /// <summary>
    /// Get the underlying Azure SecretClient for advanced operations
    /// </summary>
    public SecretClient UnderlyingClient => _secretClient;
}
