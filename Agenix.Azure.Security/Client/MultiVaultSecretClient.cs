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
using Microsoft.Extensions.Logging;

namespace Agenix.Azure.Security.Client;

/// <summary>
/// Client for retrieving secrets from multiple Azure Key Vaults
/// </summary>
public class MultiVaultSecretClient
{
    /// <summary>
    /// Logger instance
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(MultiVaultSecretClient));

    /// <summary>
    /// List of Key Vault clients to search
    /// </summary>
    private readonly List<KeyVaultSecretClient> _vaultClients;

    /// <summary>
    /// Search strategy for finding secrets
    /// </summary>
    private readonly SecretSearchStrategy _searchStrategy;

    /// <summary>
    /// Cache for found secrets to avoid repeated searches
    /// </summary>
    private readonly Dictionary<string, (string Value, string VaultUri)> _secretCache;

    /// <summary>
    /// Lock for thread-safe cache operations
    /// </summary>
    private readonly SemaphoreSlim _cacheLock = new(1, 1);

    /// <summary>
    /// Initialize MultiVaultSecretClient
    /// </summary>
    /// <param name="vaultConfigurations">List of Key Vault configurations</param>
    /// <param name="searchStrategy">Strategy for searching across vaults</param>
    public MultiVaultSecretClient(
        IEnumerable<KeyVaultConfiguration> vaultConfigurations,
        SecretSearchStrategy searchStrategy = SecretSearchStrategy.FIRST_FOUND)
    {
        ArgumentNullException.ThrowIfNull(vaultConfigurations);

        var configs = vaultConfigurations.ToList();
        if (configs.Count == 0)
            throw new ArgumentException("At least one vault configuration is required", nameof(vaultConfigurations));

        _vaultClients = configs.Select(config => new KeyVaultSecretClient(config)).ToList();
        _searchStrategy = searchStrategy;
        _secretCache = new Dictionary<string, (string, string)>();

        Log.LogInformation("Initialized MultiVaultSecretClient with {VaultCount} vaults using {Strategy} strategy",
            _vaultClients.Count, searchStrategy);
    }

    /// <summary>
    /// Initialize MultiVaultSecretClient with existing clients
    /// </summary>
    /// <param name="vaultClients">List of Key Vault clients</param>
    /// <param name="searchStrategy">Strategy for searching across vaults</param>
    public MultiVaultSecretClient(
        IEnumerable<KeyVaultSecretClient> vaultClients,
        SecretSearchStrategy searchStrategy = SecretSearchStrategy.FIRST_FOUND)
    {
        ArgumentNullException.ThrowIfNull(vaultClients);

        _vaultClients = vaultClients.ToList();
        if (_vaultClients.Count == 0)
            throw new ArgumentException("At least one vault client is required", nameof(vaultClients));

        _searchStrategy = searchStrategy;
        _secretCache = new Dictionary<string, (string, string)>();

        Log.LogInformation("Initialized MultiVaultSecretClient with {VaultCount} vault clients using {Strategy} strategy",
            _vaultClients.Count, searchStrategy);
    }

    /// <summary>
    /// Get secret value from any of the configured vaults
    /// </summary>
    /// <param name="secretName">Name of the secret</param>
    /// <param name="useCache">Whether to use cached results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Secret value and the vault URI where it was found</returns>
    public async Task<SecretResult> GetSecretAsync(string secretName, bool useCache = true, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(secretName))
            throw new ArgumentException("Secret name cannot be null or empty", nameof(secretName));

        // Check cache first
        if (useCache)
        {
            await _cacheLock.WaitAsync(cancellationToken);
            try
            {
                if (_secretCache.TryGetValue(secretName, out var cachedResult))
                {
                    Log.LogDebug("Returning cached secret '{SecretName}' from vault '{VaultUri}'",
                        secretName, cachedResult.VaultUri);
                    return new SecretResult(cachedResult.Value, cachedResult.VaultUri, true);
                }
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        Log.LogDebug("Searching for secret '{SecretName}' across {VaultCount} vaults using {Strategy} strategy",
            secretName, _vaultClients.Count, _searchStrategy);

        var result = _searchStrategy switch
        {
            SecretSearchStrategy.FIRST_FOUND => await SearchFirstFoundAsync(secretName, cancellationToken),
            SecretSearchStrategy.PARALLEL => await SearchParallelAsync(secretName, cancellationToken),
            SecretSearchStrategy.SEQUENTIAL => await SearchSequentialAsync(secretName, cancellationToken),
            SecretSearchStrategy.PRIORITY_ORDER => await SearchPriorityOrderAsync(secretName, cancellationToken),
            _ => throw new ArgumentException($"Unknown search strategy: {_searchStrategy}")
        };

        // Cache the result if found
        if (result.Found && useCache)
        {
            await _cacheLock.WaitAsync(cancellationToken);
            try
            {
                _secretCache[secretName] = (result.Value, result.VaultUri);
            }
            finally
            {
                _cacheLock.Release();
            }
        }

        return result;
    }

    /// <summary>
    /// Get secret value synchronously
    /// </summary>
    /// <param name="secretName">Name of the secret</param>
    /// <param name="useCache">Whether to use cached results</param>
    /// <returns>Secret value and the vault URI where it was found</returns>
    public SecretResult GetSecret(string secretName, bool useCache = true)
    {
        return GetSecretAsync(secretName, useCache).GetAwaiter().GetResult();
    }

    /// <summary>
    /// Search strategy: Return first found secret (fastest)
    /// </summary>
    private async Task<SecretResult> SearchFirstFoundAsync(string secretName, CancellationToken cancellationToken)
    {
        foreach (var client in _vaultClients)
        {
            try
            {
                if (await client.SecretExistsAsync(secretName, cancellationToken))
                {
                    var value = await client.GetSecretAsync(secretName, cancellationToken);
                    Log.LogDebug("Found secret '{SecretName}' in vault '{VaultUri}'", secretName, client.VaultUri);
                    return new SecretResult(value, client.VaultUri, false);
                }
            }
            catch (Exception ex)
            {
                Log.LogWarning(ex, "Error checking secret '{SecretName}' in vault '{VaultUri}'",
                    secretName, client.VaultUri);
            }
        }

        Log.LogWarning("Secret '{SecretName}' not found in any of the {VaultCount} configured vaults",
            secretName, _vaultClients.Count);
        return SecretResult.NotFound;
    }

    /// <summary>
    /// Search strategy: Search all vaults in parallel (fastest for multiple secrets)
    /// </summary>
    private async Task<SecretResult> SearchParallelAsync(string secretName, CancellationToken cancellationToken)
    {
        var tasks = _vaultClients.Select(async client =>
        {
            try
            {
                if (await client.SecretExistsAsync(secretName, cancellationToken))
                {
                    var value = await client.GetSecretAsync(secretName, cancellationToken);
                    return new SecretResult(value, client.VaultUri, false);
                }
            }
            catch (Exception ex)
            {
                Log.LogWarning(ex, "Error checking secret '{SecretName}' in vault '{VaultUri}'",
                    secretName, client.VaultUri);
            }
            return SecretResult.NotFound;
        });

        var results = await Task.WhenAll(tasks);
        var found = results.FirstOrDefault(r => r.Found);

        if (found != null)
        {
            Log.LogDebug("Found secret '{SecretName}' in vault '{VaultUri}' (parallel search)",
                secretName, found.VaultUri);
            return found;
        }

        Log.LogWarning("Secret '{SecretName}' not found in any of the {VaultCount} configured vaults",
            secretName, _vaultClients.Count);
        return SecretResult.NotFound;
    }

    /// <summary>
    /// Search strategy: Search vaults sequentially with error handling
    /// </summary>
    private async Task<SecretResult> SearchSequentialAsync(string secretName, CancellationToken cancellationToken)
    {
        var errors = new List<Exception>();

        foreach (var client in _vaultClients)
        {
            try
            {
                if (await client.SecretExistsAsync(secretName, cancellationToken))
                {
                    var value = await client.GetSecretAsync(secretName, cancellationToken);
                    Log.LogDebug("Found secret '{SecretName}' in vault '{VaultUri}'", secretName, client.VaultUri);
                    return new SecretResult(value, client.VaultUri, false);
                }
            }
            catch (Exception ex)
            {
                errors.Add(ex);
                Log.LogWarning(ex, "Error checking secret '{SecretName}' in vault '{VaultUri}'",
                    secretName, client.VaultUri);
            }
        }

        if (errors.Count == _vaultClients.Count)
        {
            var aggregateException = new AggregateException($"Failed to check secret '{secretName}' in all vaults", errors);
            Log.LogError(aggregateException, "All vault checks failed for secret '{SecretName}'", secretName);
            throw aggregateException;
        }

        Log.LogWarning("Secret '{SecretName}' not found in any of the {VaultCount} configured vaults",
            secretName, _vaultClients.Count);
        return SecretResult.NotFound;
    }

    /// <summary>
    /// Search strategy: Search in priority order (first vault has highest priority)
    /// </summary>
    private async Task<SecretResult> SearchPriorityOrderAsync(string secretName, CancellationToken cancellationToken)
    {
        // Same as FirstFound but with explicit priority ordering
        return await SearchFirstFoundAsync(secretName, cancellationToken);
    }

    /// <summary>
    /// Get multiple secrets at once
    /// </summary>
    /// <param name="secretNames">Names of secrets to retrieve</param>
    /// <param name="useCache">Whether to use cached results</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Dictionary of found secrets</returns>
    public async Task<Dictionary<string, SecretResult>> GetSecretsAsync(
        IEnumerable<string> secretNames,
        bool useCache = true,
        CancellationToken cancellationToken = default)
    {
        if (secretNames == null)
            throw new ArgumentNullException(nameof(secretNames));

        var secretNamesList = secretNames.ToList();
        var results = new Dictionary<string, SecretResult>();

        var tasks = secretNamesList.Select(async secretName =>
        {
            try
            {
                var result = await GetSecretAsync(secretName, useCache, cancellationToken);
                return new KeyValuePair<string, SecretResult>(secretName, result);
            }
            catch (Exception ex)
            {
                Log.LogError(ex, "Failed to retrieve secret '{SecretName}'", secretName);
                return new KeyValuePair<string, SecretResult>(secretName, SecretResult.NotFound);
            }
        });

        var completedTasks = await Task.WhenAll(tasks);

        foreach (var result in completedTasks)
        {
            results[result.Key] = result.Value;
        }

        return results;
    }

    /// <summary>
    /// Clear the secret cache
    /// </summary>
    public async Task ClearCacheAsync()
    {
        await _cacheLock.WaitAsync();
        try
        {
            _secretCache.Clear();
            Log.LogDebug("Cleared secret cache");
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    /// <summary>
    /// Get cache statistics
    /// </summary>
    public async Task<(int CachedSecrets, string[] SecretNames)> GetCacheStatsAsync()
    {
        await _cacheLock.WaitAsync();
        try
        {
            return (_secretCache.Count, _secretCache.Keys.ToArray());
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    /// <summary>
    /// Get configured vault URIs
    /// </summary>
    public string[] VaultUris => _vaultClients.Select(c => c.VaultUri).ToArray();
}

/// <summary>
/// Result of a secret search operation
/// </summary>
public class SecretResult
{
    public string Value { get; }
    public string VaultUri { get; }
    public bool Found { get; }
    public bool FromCache { get; }

    /// <summary>
    /// Represents the result of a secret retrieval operation, containing the secret value, the originating vault's URI, and metadata about the operation.
    /// </summary>
    public SecretResult(string value, string vaultUri, bool fromCache)
    {
        Value = value;
        VaultUri = vaultUri;
        Found = true;
        FromCache = fromCache;
    }

    private SecretResult()
    {
        Value = string.Empty;
        VaultUri = string.Empty;
        Found = false;
        FromCache = false;
    }

    /// <summary>
    /// Represents the result of a failed secret search operation, indicating that the secret was not found.
    /// </summary>
    public static SecretResult NotFound => new();

    /// <summary>
    /// Returns a string representation of the secret retrieval result, indicating whether the secret was found,
    /// the vault it originated from, and if it was retrieved from cache.
    /// </summary>
    /// <returns>
    /// A string describing the secret retrieval result. If the secret was found, the string includes
    /// the originating vault URI and cache information; otherwise, it indicates that the secret was not found.
    /// </returns>
    public override string ToString()
    {
        return Found
            ? $"Found in {VaultUri}{(FromCache ? " (cached)" : "")}"
            : "Not found";
    }
}

/// <summary>
/// Search strategies for finding secrets across multiple vaults
/// </summary>
public enum SecretSearchStrategy
{
    /// <summary>
    /// Return the first secret found (fastest for a single secret)
    /// </summary>
    FIRST_FOUND,

    /// <summary>
    /// Search all vaults in parallel (fastest for multiple secrets)
    /// </summary>
    PARALLEL,

    /// <summary>
    /// Search vaults sequentially with detailed error handling
    /// </summary>
    SEQUENTIAL,

    /// <summary>
    /// Search in priority order (first vault has the highest priority)
    /// </summary>
    PRIORITY_ORDER
}
