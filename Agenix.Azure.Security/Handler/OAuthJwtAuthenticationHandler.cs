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

using System.Net.Http.Headers;
using Agenix.Api.Log;
using Agenix.Azure.Security.Client;
using Agenix.Azure.Security.Configuration;
using Microsoft.Extensions.Logging;

namespace Agenix.Azure.Security.Handler;

/// <summary>
/// DelegatingHandler for OAuth JWT authentication in HTTP requests
/// </summary>
public class OAuthJwtAuthenticationHandler : DelegatingHandler
{
    private static readonly ILogger Log = LogManager.GetLogger(typeof(OAuthJwtAuthenticationHandler));

    private readonly OAuthTokenClient _tokenClient;
    private readonly OAuthClientConfiguration _oauthConfiguration;
    private readonly OAuthJwtHandlerOptions _options;
    private bool _disposed;

    /// <summary>
    /// A delegating handler for managing and appending OAuth JWT authentication tokens to HTTP requests.
    /// </summary>
    public OAuthJwtAuthenticationHandler(
        OAuthTokenClient tokenClient,
        OAuthClientConfiguration oauthConfiguration,
        OAuthJwtHandlerOptions? options = null)
    {
        _tokenClient = tokenClient ?? throw new ArgumentNullException(nameof(tokenClient));
        _oauthConfiguration = oauthConfiguration ?? throw new ArgumentNullException(nameof(oauthConfiguration));
        _options = options ?? new OAuthJwtHandlerOptions();

        // Validate configuration on creation
        _oauthConfiguration.Validate();
    }

    /// <summary>
    /// Sends an HTTP request and processes the response, including adding OAuth JWT authentication headers if applicable.
    /// </summary>
    /// <param name="request">
    /// The HTTP request message to send.
    /// </param>
    /// <param name="cancellationToken">
    /// The cancellation token to cancel the operation.
    /// </param>
    /// <returns>
    /// A task that represents the asynchronous operation and contains the HTTP response message.
    /// </returns>
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        // Skip if request already has authorization header and we're not forcing override
        if (!_options.OverrideExistingAuthHeader && request.Headers.Authorization != null)
        {
            Log.LogDebug("Skipping OAuth JWT authentication - Authorization header already present for {Method} {Uri}",
                request.Method, request.RequestUri);
            return await base.SendAsync(request, cancellationToken);
        }

        // Skip if the URL matches any excluded patterns
        if (IsUrlExcluded(request.RequestUri))
        {
            Log.LogDebug("Skipping OAuth JWT authentication - URL excluded: {Method} {Uri}",
                request.Method, request.RequestUri);
            return await base.SendAsync(request, cancellationToken);
        }

        try
        {
            // Get OAuth JWT token
            var token = await _tokenClient.GetTokenAsync(_oauthConfiguration, _options.TokenCacheExpiry, cancellationToken);

            if (string.IsNullOrEmpty(token))
            {
                Log.LogWarning("Failed to acquire OAuth JWT token for request: {Method} {Uri}",
                    request.Method, request.RequestUri);

                if (_options.FailOnTokenAcquisitionError)
                {
                    throw new InvalidOperationException("Failed to acquire OAuth JWT token");
                }

                return await base.SendAsync(request, cancellationToken);
            }

            // Add Authorization header
            request.Headers.Authorization = new AuthenticationHeaderValue(_options.AuthenticationScheme, token);

            Log.LogDebug("Added OAuth JWT token to request: {Method} {Uri}", request.Method, request.RequestUri);

            // Add token to custom header if specified
            if (!string.IsNullOrEmpty(_options.CustomTokenHeaderName))
            {
                var headerValue = _options.IncludeSchemeInCustomHeader
                    ? $"{_options.AuthenticationScheme} {token}"
                    : token;
                request.Headers.TryAddWithoutValidation(_options.CustomTokenHeaderName, headerValue);
                Log.LogDebug("Added custom token header {Header} to request", _options.CustomTokenHeaderName);
            }

            var response = await base.SendAsync(request, cancellationToken);

            // Handle 401 Unauthorized - token might be expired
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized && _options.RetryOnUnauthorized)
            {
                Log.LogWarning("Received 401 Unauthorized - attempting token refresh for {Method} {Uri}",
                    request.Method, request.RequestUri);

                // Clearly cached token and retry once
                _tokenClient.RemoveToken(_oauthConfiguration.GetCacheKey());

                var newToken = await _tokenClient.GetTokenAsync(_oauthConfiguration, _options.TokenCacheExpiry, cancellationToken);
                if (!string.IsNullOrEmpty(newToken))
                {
                    // Update Authorization header
                    request.Headers.Authorization = new AuthenticationHeaderValue(_options.AuthenticationScheme, newToken);

                    // Update custom header if specified
                    if (!string.IsNullOrEmpty(_options.CustomTokenHeaderName))
                    {
                        request.Headers.Remove(_options.CustomTokenHeaderName);
                        var headerValue = _options.IncludeSchemeInCustomHeader
                            ? $"{_options.AuthenticationScheme} {newToken}"
                            : newToken;
                        request.Headers.TryAddWithoutValidation(_options.CustomTokenHeaderName, headerValue);
                    }

                    Log.LogDebug("Retrying request with refreshed OAuth JWT token: {Method} {Uri}",
                        request.Method, request.RequestUri);
                    response = await base.SendAsync(request, cancellationToken);
                }
                else
                {
                    Log.LogWarning("Failed to refresh OAuth JWT token for retry: {Method} {Uri}",
                        request.Method, request.RequestUri);
                }
            }

            return response;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            Log.LogDebug("OAuth JWT authentication cancelled for request: {Method} {Uri}",
                request.Method, request.RequestUri);
            throw;
        }
        catch (Exception ex)
        {
            Log.LogError(ex, "Error in OAuth JWT authentication handler for request: {Method} {Uri}",
                request.Method, request.RequestUri);

            if (_options.FailOnTokenAcquisitionError)
            {
                throw;
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }

    private bool IsUrlExcluded(Uri? uri)
    {
        if (uri == null || _options.ExcludedUrlPatterns.Count == 0)
            return false;

        var url = uri.ToString();
        return _options.ExcludedUrlPatterns.Any(pattern =>
            url.Contains(pattern, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Releases the resources used by the <see cref="OAuthJwtAuthenticationHandler"/> instance.
    /// </summary>
    /// <param name="disposing">
    /// A boolean value indicating whether to release managed resources as well as unmanaged resources.
    /// <c>true</c> to release both managed and unmanaged resources;
    /// <c>false</c> to release only unmanaged resources.
    /// </param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && !_disposed)
        {
            // OAuthTokenClient will be disposed of by its owner
            _disposed = true;
        }
        base.Dispose(disposing);
    }
}

/// <summary>
/// Options for OAuth JWT authentication handler
/// </summary>
public class OAuthJwtHandlerOptions
{
    /// <summary>
    /// Authentication scheme to use in Authorization header (default: "Bearer")
    /// </summary>
    public string AuthenticationScheme { get; set; } = "Bearer";

    /// <summary>
    /// Whether to override existing Authorization header
    /// </summary>
    public bool OverrideExistingAuthHeader { get; set; } = false;

    /// <summary>
    /// Whether to retry request on 401 Unauthorized response
    /// </summary>
    public bool RetryOnUnauthorized { get; set; } = true;

    /// <summary>
    /// Whether to fail on token acquisition error
    /// </summary>
    public bool FailOnTokenAcquisitionError { get; set; } = true;

    /// <summary>
    /// Token cache expiry time (default: 55 minutes)
    /// </summary>
    public TimeSpan? TokenCacheExpiry { get; set; } = TimeSpan.FromMinutes(55);

    /// <summary>
    /// Custom header name to include token (optional)
    /// </summary>
    public string? CustomTokenHeaderName { get; set; }

    /// <summary>
    /// Whether to include scheme in custom header value
    /// </summary>
    public bool IncludeSchemeInCustomHeader { get; set; } = false;

    /// <summary>
    /// URL patterns to exclude from authentication
    /// </summary>
    public List<string> ExcludedUrlPatterns { get; set; } = [];
}
