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
using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Agenix.Api.Log;
using Agenix.Azure.Security.Configuration;
using Agenix.Azure.Security.Message;
using Microsoft.Extensions.Logging;

namespace Agenix.Azure.Security.Client;

/// <summary>
/// OAuth token client for acquiring and managing access tokens
/// </summary>
public class OAuthTokenClient : IDisposable
{
    private static readonly ILogger Log = LogManager.GetLogger(typeof(OAuthTokenClient));

    private readonly HttpClient _httpClient;
    private readonly bool _ownsHttpClient;
    private readonly JwtSecurityTokenHandler _jwtHandler;
    private readonly ConcurrentDictionary<string, CachedToken> _tokenCache;
    private readonly SemaphoreSlim _refreshSemaphore;
    private bool _disposed;

    /// <summary>
    /// OAuth token client for acquiring and managing access tokens
    /// </summary>
    public OAuthTokenClient(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
        _ownsHttpClient = httpClient == null;
        _jwtHandler = new JwtSecurityTokenHandler();
        _tokenCache = new ConcurrentDictionary<string, CachedToken>();
        _refreshSemaphore = new SemaphoreSlim(1, 1);
    }

    /// <summary>
    /// Get OAuth token with automatic caching
    /// </summary>
    public async Task<string?> GetTokenAsync(OAuthClientConfiguration configuration, TimeSpan? cacheExpiry = null, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        configuration.Validate();

        var cacheKey = configuration.GetCacheKey();
        var expiry = cacheExpiry ?? TimeSpan.FromMinutes(55);

        // Check cache first
        if (_tokenCache.TryGetValue(cacheKey, out var cachedToken))
        {
            if (cachedToken.ExpiresAt > DateTime.UtcNow && !IsTokenExpired(cachedToken.Token))
            {
                Log.LogDebug("Returning cached OAuth token for key: {Key}", cacheKey);
                return cachedToken.Token;
            }

            Log.LogDebug("Cached OAuth token expired for key: {Key}", cacheKey);
            _tokenCache.TryRemove(cacheKey, out _);
        }

        // Acquire new token
        await _refreshSemaphore.WaitAsync(cancellationToken);
        try
        {
            // Double-check after acquiring a lock
            if (_tokenCache.TryGetValue(cacheKey, out cachedToken))
            {
                if (cachedToken.ExpiresAt > DateTime.UtcNow && !IsTokenExpired(cachedToken.Token))
                {
                    return cachedToken.Token;
                }
                _tokenCache.TryRemove(cacheKey, out _);
            }

            Log.LogInformation("Acquiring new OAuth token for key: {Key}", cacheKey);
            var result = await AcquireTokenAsync(configuration, cancellationToken);

            if (result.IsSuccess && !string.IsNullOrEmpty(result.AccessToken))
            {
                var cacheUntil = result.ExpiresAt?.Subtract(TimeSpan.FromMinutes(5)) ?? DateTime.UtcNow.Add(expiry);
                _tokenCache.TryAdd(cacheKey, new CachedToken(result.AccessToken, cacheUntil));

                Log.LogInformation("OAuth token acquired and cached for key: {Key}", cacheKey);
                return result.AccessToken;
            }

            Log.LogWarning("Failed to acquire OAuth token for key: {Key} - {Error}: {Description}",
                cacheKey, result.Error, result.ErrorDescription);
            return null;
        }
        finally
        {
            _refreshSemaphore.Release();
        }
    }

    /// <summary>
    /// Acquire access token using client credentials flow
    /// </summary>
    public async Task<OAuthTokenResult> AcquireTokenAsync(OAuthClientConfiguration configuration, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        configuration.Validate();

        var retryCount = 0;
        var retryConfig = configuration.RetryConfiguration;

        while (retryCount <= retryConfig.MaxRetries)
        {
            try
            {
                Log.LogDebug("Attempting to acquire OAuth token (attempt {Attempt}/{MaxAttempts})",
                    retryCount + 1, retryConfig.MaxRetries + 1);

                var result = await ExecuteTokenRequestAsync(configuration, cancellationToken);

                if (result.IsSuccess)
                {
                    Log.LogInformation("Successfully acquired OAuth token for client: {ClientId}", configuration.ClientId);
                    return result;
                }

                Log.LogWarning("OAuth token request failed: {Error} - {Description}", result.Error, result.ErrorDescription);

                // Don't retry on client errors (4xx)
                if (IsClientError(result))
                {
                    Log.LogInformation("Not retrying OAuth request due to client error: {Error}", result.Error);
                    return result;
                }

                if (retryCount < retryConfig.MaxRetries)
                {
                    var delay = retryConfig.GetDelay(retryCount);
                    Log.LogInformation("Retrying OAuth token request in {Delay}ms", delay.TotalMilliseconds);
                    await Task.Delay(delay, cancellationToken);
                }

                retryCount++;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                Log.LogWarning("OAuth token request was cancelled");
                return OAuthTokenResult.Failed("cancelled", "Request was cancelled");
            }
            catch (Exception ex)
            {
                Log.LogError(ex, "Unexpected error during OAuth token request (attempt {Attempt})", retryCount + 1);

                if (retryCount >= retryConfig.MaxRetries)
                {
                    return OAuthTokenResult.Failed("request_failed", "Maximum retry attempts exceeded", ex);
                }

                if (retryCount < retryConfig.MaxRetries)
                {
                    var delay = retryConfig.GetDelay(retryCount);
                    await Task.Delay(delay, cancellationToken);
                }

                retryCount++;
            }
        }

        return OAuthTokenResult.Failed("max_retries_exceeded", "Maximum retry attempts exceeded");
    }

    /// <summary>
    /// Parse JWT token to get claims and expiration
    /// </summary>
    public JwtSecurityToken? ParseToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        try
        {
            return _jwtHandler.ReadJwtToken(token);
        }
        catch (Exception ex)
        {
            Log.LogWarning(ex, "Failed to parse JWT token");
            return null;
        }
    }

    /// <summary>
    /// Check if token is expired
    /// </summary>
    public bool IsTokenExpired(string token, TimeSpan? buffer = null)
    {
        if (string.IsNullOrWhiteSpace(token))
            return true;

        try
        {
            var jwtToken = ParseToken(token);
            if (jwtToken == null) return true;

            var expiryBuffer = buffer ?? TimeSpan.FromMinutes(5);
            return jwtToken.ValidTo <= DateTime.UtcNow.Add(expiryBuffer);
        }
        catch
        {
            return true;
        }
    }

    /// <summary>
    /// Get token expiration time
    /// </summary>
    public DateTime? GetTokenExpiration(string token)
    {
        var jwtToken = ParseToken(token);
        return jwtToken?.ValidTo;
    }

    /// <summary>
    /// Get remaining token lifetime
    /// </summary>
    public TimeSpan? GetRemainingTokenLifetime(string token)
    {
        var expiration = GetTokenExpiration(token);
        if (!expiration.HasValue) return null;

        var remaining = expiration.Value - DateTime.UtcNow;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    /// <summary>
    /// Remove token from cache
    /// </summary>
    public void RemoveToken(string cacheKey)
    {
        _tokenCache.TryRemove(cacheKey, out _);
        Log.LogDebug("OAuth token removed from cache for key: {Key}", cacheKey);
    }

    /// <summary>
    /// Clear all cached tokens
    /// </summary>
    public void ClearCache()
    {
        _tokenCache.Clear();
        Log.LogInformation("OAuth token cache cleared");
    }

    /// <summary>
    /// Get cache statistics
    /// </summary>
    public OAuthCacheStatistics GetCacheStatistics()
    {
        var now = DateTime.UtcNow;
        var tokens = _tokenCache.Values.ToList();

        return new OAuthCacheStatistics
        {
            TotalTokens = tokens.Count,
            ExpiredTokens = tokens.Count(t => t.ExpiresAt <= now),
            ValidTokens = tokens.Count(t => t.ExpiresAt > now),
            OldestToken = tokens.MinBy(t => t.CachedAt)?.CachedAt,
            NewestToken = tokens.MaxBy(t => t.CachedAt)?.CachedAt
        };
    }

    private async Task<OAuthTokenResult> ExecuteTokenRequestAsync(OAuthClientConfiguration configuration, CancellationToken cancellationToken)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(configuration.Timeout);

        var formData = configuration.GetFormData();
        var content = new FormUrlEncodedContent(formData);

        using var request = new HttpRequestMessage(HttpMethod.Post, configuration.TokenEndpoint)
        {
            Content = content
        };

        // Add custom headers
        foreach (var header in configuration.CustomHeaders)
        {
            request.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        Log.LogDebug("Sending OAuth token request to: {Endpoint} for client: {ClientId}",
            configuration.TokenEndpoint, configuration.ClientId);

        using var response = await _httpClient.SendAsync(request, cts.Token);
        var responseContent = await response.Content.ReadAsStringAsync(cts.Token);

        Log.LogDebug("OAuth token response status: {StatusCode} for client: {ClientId}",
            response.StatusCode, configuration.ClientId);

        if (response.IsSuccessStatusCode)
        {
            try
            {
                var tokenResponse = JsonSerializer.Deserialize<OAuthTokenResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (tokenResponse?.HasError == true)
                {
                    return OAuthTokenResult.Failed(tokenResponse.Error ?? "unknown_error", tokenResponse.ErrorDescription);
                }

                if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
                {
                    return OAuthTokenResult.Failed("invalid_response", "No access token in response");
                }

                return OAuthTokenResult.Success(tokenResponse);
            }
            catch (JsonException ex)
            {
                Log.LogError(ex, "Failed to parse OAuth token response");
                return OAuthTokenResult.Failed("invalid_response", "Failed to parse token response", ex);
            }
        }
        else
        {
            try
            {
                var errorResponse = JsonSerializer.Deserialize<OAuthTokenResponse>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return OAuthTokenResult.Failed(
                    errorResponse?.Error ?? $"http_error_{(int)response.StatusCode}",
                    errorResponse?.ErrorDescription ?? response.ReasonPhrase);
            }
            catch
            {
                return OAuthTokenResult.Failed(
                    $"http_error_{(int)response.StatusCode}",
                    $"HTTP {(int)response.StatusCode}: {response.ReasonPhrase}");
            }
        }
    }

    private static bool IsClientError(OAuthTokenResult result)
    {
        return result.Error switch
        {
            "invalid_request" => true,
            "invalid_client" => true,
            "invalid_grant" => true,
            "unauthorized_client" => true,
            "unsupported_grant_type" => true,
            "invalid_scope" => true,
            _ when result.Error?.StartsWith("http_error_4") == true => true,
            _ => false
        };
    }

    /// <summary>
    /// Releases all resources used by the OAuthTokenClient, including the HttpClient
    /// if it was created internally, and other disposable members.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;

        _refreshSemaphore?.Dispose();
        if (_ownsHttpClient)
        {
            _httpClient?.Dispose();
        }

        _disposed = true;
    }

    private record CachedToken(string Token, DateTime ExpiresAt)
    {
        public DateTime CachedAt { get; } = DateTime.UtcNow;
    }
}

/// <summary>
/// OAuth cache statistics
/// </summary>
public class OAuthCacheStatistics
{
    public int TotalTokens { get; init; }
    public int ValidTokens { get; init; }
    public int ExpiredTokens { get; init; }
    public DateTime? OldestToken { get; init; }
    public DateTime? NewestToken { get; init; }

    /// <summary>
    /// Returns a string representation of the object.
    /// </summary>
    /// <return>
    /// A string that represents the current object.
    /// </return>
    public override string ToString()
    {
        return $"OAuth Cache: {ValidTokens} valid, {ExpiredTokens} expired, {TotalTokens} total tokens";
    }
}
