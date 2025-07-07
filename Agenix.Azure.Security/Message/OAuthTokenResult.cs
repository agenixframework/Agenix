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

namespace Agenix.Azure.Security.Message;

/// <summary>
/// Result of OAuth token operation
/// </summary>
public class OAuthTokenResult
{
    /// <summary>
    /// Whether the operation was successful
    /// </summary>
    public bool IsSuccess { get; init; }

    /// <summary>
    /// Access token
    /// </summary>
    public string? AccessToken { get; init; }

    /// <summary>
    /// Token type
    /// </summary>
    public string? TokenType { get; init; }

    /// <summary>
    /// Token expiration time
    /// </summary>
    public DateTime? ExpiresAt { get; init; }

    /// <summary>
    /// Granted scopes
    /// </summary>
    public List<string>? GrantedScopes { get; init; }

    /// <summary>
    /// Error code
    /// </summary>
    public string? Error { get; init; }

    /// <summary>
    /// Error description
    /// </summary>
    public string? ErrorDescription { get; init; }

    /// <summary>
    /// Exception that occurred (if any)
    /// </summary>
    public Exception? Exception { get; init; }

    /// <summary>
    /// Additional data from the response
    /// </summary>
    public Dictionary<string, object>? AdditionalData { get; init; }

    /// <summary>
    /// Time when the result was created
    /// </summary>
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Create successful result
    /// </summary>
    public static OAuthTokenResult Success(OAuthTokenResponse response)
    {
        return new OAuthTokenResult
        {
            IsSuccess = true,
            AccessToken = response.AccessToken,
            TokenType = response.EffectiveTokenType,
            ExpiresAt = response.ExpiresAt,
            GrantedScopes = response.GetGrantedScopes(),
            AdditionalData = response.AdditionalData
        };
    }

    /// <summary>
    /// Create failed result
    /// </summary>
    public static OAuthTokenResult Failed(string error, string? errorDescription = null, Exception? exception = null)
    {
        return new OAuthTokenResult
        {
            IsSuccess = false,
            Error = error,
            ErrorDescription = errorDescription,
            Exception = exception
        };
    }

    /// <summary>
    /// Create failed result from OAuth response
    /// </summary>
    public static OAuthTokenResult Failed(OAuthTokenResponse response)
    {
        return new OAuthTokenResult
        {
            IsSuccess = false,
            Error = response.Error,
            ErrorDescription = response.ErrorDescription,
            AdditionalData = response.AdditionalData
        };
    }

    /// <summary>
    /// Get remaining token lifetime
    /// </summary>
    public TimeSpan? GetRemainingLifetime()
    {
        if (!IsSuccess || !ExpiresAt.HasValue)
            return null;

        var remaining = ExpiresAt.Value - DateTime.UtcNow;
        return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
    }

    /// <summary>
    /// Check if token is expired or about to expire
    /// </summary>
    public bool IsExpired(TimeSpan? buffer = null)
    {
        if (!IsSuccess || !ExpiresAt.HasValue)
            return true;

        var bufferTime = buffer ?? TimeSpan.FromMinutes(5);
        return ExpiresAt.Value <= DateTime.UtcNow.Add(bufferTime);
    }

    /// <summary>
    /// Returns a string that represents the current object, describing the status of the OAuth token operation.
    /// </summary>
    /// <returns>
    /// A string indicating whether the token operation was successful, its expiration information, and granted scopes
    /// when successful, or the error details if the operation failed.
    /// </returns>
    public override string ToString()
    {
        if (IsSuccess)
        {
            var remaining = GetRemainingLifetime();
            return $"OAuth Success: Token expires in {remaining?.TotalMinutes:F1} minutes, scopes: {string.Join(", ", GrantedScopes ?? new List<string>())}";
        }

        return $"OAuth Failed: {Error} - {ErrorDescription}";
    }
}
