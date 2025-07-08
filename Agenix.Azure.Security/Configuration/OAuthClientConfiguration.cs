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

namespace Agenix.Azure.Security.Configuration;

/// <summary>
/// Configuration for OAuth 2.0 client credentials flow
/// </summary>
public class OAuthClientConfiguration
{
    /// <summary>
    /// OAuth token endpoint URL
    /// </summary>
    public string TokenEndpoint { get; set; } = string.Empty;

    /// <summary>
    /// OAuth client ID
    /// </summary>
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// OAuth client secret
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;

    /// <summary>
    /// OAuth scopes to request
    /// </summary>
    public List<string> Scopes { get; set; } = new();

    /// <summary>
    /// Additional parameters to include in token request
    /// </summary>
    public Dictionary<string, string> AdditionalParameters { get; set; } = new();

    /// <summary>
    /// Custom headers to include in token request
    /// </summary>
    public Dictionary<string, string> CustomHeaders { get; set; } = new();

    /// <summary>
    /// Request timeout
    /// </summary>
    public TimeSpan Timeout { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Retry configuration
    /// </summary>
    public OAuthRetryConfiguration RetryConfiguration { get; set; } = new();

    /// <summary>
    /// Get form data for OAuth token request
    /// </summary>
    public Dictionary<string, string> GetFormData()
    {
        var formData = new Dictionary<string, string>
        {
            { "grant_type", "client_credentials" },
            { "client_id", ClientId },
            { "client_secret", ClientSecret }
        };

        // Add scope if specified
        if (Scopes.Count > 0)
        {
            formData["scope"] = string.Join(" ", Scopes);
        }

        // Add additional parameters
        foreach (var parameter in AdditionalParameters)
        {
            formData[parameter.Key] = parameter.Value;
        }

        return formData;
    }

    /// <summary>
    /// Get cache key for this configuration
    /// </summary>
    public string GetCacheKey()
    {
        var key = $"{TokenEndpoint}|{ClientId}|{string.Join(",", Scopes.OrderBy(s => s))}";

        // Include additional parameters in a cache key
        if (AdditionalParameters.Count > 0)
        {
            var additionalParams = string.Join(",", AdditionalParameters.OrderBy(kv => kv.Key).Select(kv => $"{kv.Key}={kv.Value}"));
            key += $"|{additionalParams}";
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
        if (string.IsNullOrWhiteSpace(TokenEndpoint))
            throw new InvalidOperationException("TokenEndpoint is required");

        if (string.IsNullOrWhiteSpace(ClientId))
            throw new InvalidOperationException("ClientId is required");

        if (string.IsNullOrWhiteSpace(ClientSecret))
            throw new InvalidOperationException("ClientSecret is required");

        if (!Uri.TryCreate(TokenEndpoint, UriKind.Absolute, out var uri))
            throw new InvalidOperationException("TokenEndpoint must be a valid absolute URL");

        if (uri.Scheme != "https" && uri.Scheme != "http")
            throw new InvalidOperationException("TokenEndpoint must use HTTP or HTTPS scheme");

        if (Timeout <= TimeSpan.Zero)
            throw new InvalidOperationException("Timeout must be positive");
    }
}

/// <summary>
/// Retry configuration for OAuth token requests
/// </summary>
public class OAuthRetryConfiguration
{
    /// <summary>
    /// Maximum number of retry attempts
    /// </summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>
    /// Initial delay between retries
    /// </summary>
    public TimeSpan InitialDelay { get; set; } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Maximum delay between retries
    /// </summary>
    public TimeSpan MaxDelay { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Delay multiplier for exponential backoff
    /// </summary>
    public double DelayMultiplier { get; set; } = 2.0;

    /// <summary>
    /// Whether to add jitter to retry delays
    /// </summary>
    public bool UseJitter { get; set; } = true;

    /// <summary>
    /// Get retry delay with optional jitter
    /// </summary>
    public TimeSpan GetDelay(int attempt)
    {
        var delay = TimeSpan.FromMilliseconds(
            Math.Min(InitialDelay.TotalMilliseconds * Math.Pow(DelayMultiplier, attempt), MaxDelay.TotalMilliseconds));

        if (UseJitter)
        {
            var jitter = Random.Shared.NextDouble() * 0.1; // 10% jitter
            delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * (1 + jitter));
        }

        return delay;
    }
}
