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

using Agenix.Azure.Security.Configuration;

namespace Agenix.Azure.Security.Client;

/// <summary>
/// Factory for creating Azure Key Vault SecretClient instances (simplified wrapper)
/// </summary>
public static class KeyVaultClientFactory
{
    /// <summary>
    /// Create KeyVaultSecretClient with basic authentication
    /// </summary>
    public static KeyVaultSecretClient CreateClient(
        string vaultUri,
        string tenantId,
        string clientId,
        string clientSecret,
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

        return new KeyVaultSecretClient(configuration);
    }

    /// <summary>
    /// Create KeyVaultSecretClient with full configuration
    /// </summary>
    public static KeyVaultSecretClient CreateClient(KeyVaultConfiguration configuration)
    {
        return new KeyVaultSecretClient(configuration);
    }

    /// <summary>
    /// Create KeyVaultSecretClient with retry configuration
    /// </summary>
    public static KeyVaultSecretClient CreateClientWithRetry(
        string vaultUri,
        string tenantId,
        string clientId,
        string clientSecret,
        int maxRetries = 3,
        TimeSpan? initialDelay = null,
        string? authority = null)
    {
        var configuration = new KeyVaultConfiguration
        {
            VaultUri = vaultUri,
            TenantId = tenantId,
            ClientId = clientId,
            ClientSecret = clientSecret,
            Authority = authority,
            MaxRetries = maxRetries,
            RetryDelay = initialDelay ?? TimeSpan.FromSeconds(1)
        };

        return new KeyVaultSecretClient(configuration);
    }
}
