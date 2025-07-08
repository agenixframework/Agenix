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

using System.Text.Json.Serialization;

namespace Agenix.Azure.Security.Message;

/// <summary>
/// OAuth 2.0 token response
/// </summary>
public class OAuthTokenResponse
{
    /// <summary>
    /// Access token
    /// </summary>
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; set; }

    /// <summary>
    /// Token type (usually "Bearer")
    /// </summary>
    [JsonPropertyName("token_type")]
    public string? TokenType { get; set; }

    /// <summary>
    /// Token expiration in seconds
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int? ExpiresIn { get; set; }

    /// <summary>
    /// Refresh token (not used in client credentials flow)
    /// </summary>
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; set; }

    /// <summary>
    /// Granted scopes
    /// </summary>
    [JsonPropertyName("scope")]
    public string? Scope { get; set; }

    /// <summary>
    /// Error code
    /// </summary>
    [JsonPropertyName("error")]
    public string? Error { get; set; }

    /// <summary>
    /// Error description
    /// </summary>
    [JsonPropertyName("error_description")]
    public string? ErrorDescription { get; set; }

    /// <summary>
    /// Error URI
    /// </summary>
    [JsonPropertyName("error_uri")]
    public string? ErrorUri { get; set; }

    /// <summary>
    /// Additional properties
    /// </summary>
    [JsonExtensionData]
    public Dictionary<string, object>? AdditionalData { get; set; }

    /// <summary>
    /// Check if response contains an error
    /// </summary>
    [JsonIgnore]
    public bool HasError => !string.IsNullOrEmpty(Error);

    /// <summary>
    /// Check if response is successful
    /// </summary>
    [JsonIgnore]
    public bool IsSuccess => !HasError && !string.IsNullOrEmpty(AccessToken);

    /// <summary>
    /// Get token expiration time
    /// </summary>
    [JsonIgnore]
    public DateTime? ExpiresAt => ExpiresIn.HasValue ? DateTime.UtcNow.AddSeconds(ExpiresIn.Value) : null;

    /// <summary>
    /// Get granted scopes as list
    /// </summary>
    public List<string> GetGrantedScopes()
    {
        return string.IsNullOrEmpty(Scope)
            ? []
            : Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
    }

    /// <summary>
    /// Get effective token type (defaults to "Bearer")
    /// </summary>
    [JsonIgnore]
    public string EffectiveTokenType => TokenType ?? "Bearer";

    /// <summary>
    /// Returns a string representation of the OAuth token response, indicating
    /// success with token details or error information in case of failure.
    /// </summary>
    /// <returns>
    /// A string representation of the current object. If there is an error,
    /// the string will represent the error details. Otherwise, it will contain
    /// information about the token type, expiry, and scope.
    /// </returns>
    public override string ToString()
    {
        if (HasError)
        {
            return $"OAuth Error: {Error} - {ErrorDescription}";
        }

        return $"OAuth Success: {EffectiveTokenType} token, expires in {ExpiresIn}s, scope: {Scope}";
    }
}
