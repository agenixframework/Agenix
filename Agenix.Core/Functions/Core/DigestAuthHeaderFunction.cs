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

using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Functions;

namespace Agenix.Core.Functions.Core;

/// <summary>
///     Function creates digest authentication HTTP header with given security parameters:
///     username, password, realm, noncekey, method, uri, opaque, algorithm
/// </summary>
public class DigestAuthHeaderFunction : IFunction
{
    /// <summary>
    ///     Nonce is valid for 60 seconds
    /// </summary>
    private long _nonceValidity = 60000L;

    /// <summary>
    ///     Executes the digest auth header function.
    /// </summary>
    /// <param name="parameterList">
    ///     List of parameters:
    ///     [0] - username
    ///     [1] - password
    ///     [2] - realm
    ///     [3] - noncekey
    ///     [4] - method
    ///     [5] - uri
    ///     [6] - opaque
    ///     [7] - algorithm
    /// </param>
    /// <param name="testContext">Test context</param>
    /// <returns>The digest authentication header as a string</returns>
    /// <exception cref="InvalidFunctionUsageException">Thrown when parameters are not enough</exception>
    public string Execute(List<string> parameterList, TestContext testContext)
    {
        if (parameterList == null || parameterList.Count < 8)
        {
            throw new InvalidFunctionUsageException(
                "Function parameters not set correctly - need parameters: username,password,realm,noncekey,method,uri,opaque,algorithm");
        }

        var authorizationHeader = new StringBuilder();

        var username = parameterList[0];
        var password = parameterList[1];
        var realm = parameterList[2];
        var noncekey = parameterList[3];
        var method = parameterList[4];
        var uri = parameterList[5];
        var opaque = parameterList[6];
        var algorithm = parameterList[7];

        var digest1 = username + ":" + realm + ":" + password;
        var digest2 = method + ":" + uri;

        var expirationTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + _nonceValidity;
        var nonce = Convert.ToBase64String(
            Encoding.UTF8.GetBytes(expirationTime + ":" + GetDigestHex(algorithm, expirationTime + ":" + noncekey)));

        authorizationHeader.Append("Digest username=");
        authorizationHeader.Append(username);
        authorizationHeader.Append(",realm=");
        authorizationHeader.Append(realm);
        authorizationHeader.Append(",nonce=");
        authorizationHeader.Append(nonce);
        authorizationHeader.Append(",uri=");
        authorizationHeader.Append(uri);
        authorizationHeader.Append(",response=");
        authorizationHeader.Append(GetDigestHex(algorithm,
            GetDigestHex(algorithm, digest1) + ":" + nonce + ":" + GetDigestHex(algorithm, digest2)));
        authorizationHeader.Append(",opaque=");
        authorizationHeader.Append(GetDigestHex(algorithm, opaque));
        authorizationHeader.Append(",algorithm=");
        authorizationHeader.Append(algorithm);

        return authorizationHeader.ToString();
    }

    /// <summary>
    ///     Generates digest hexadecimal string representation of a key with given algorithm.
    /// </summary>
    /// <param name="algorithm">The hash algorithm to use</param>
    /// <param name="key">The key to hash</param>
    /// <returns>Hexadecimal string representation of the hash</returns>
    /// <exception cref="AgenixSystemException">Thrown when algorithm is not supported</exception>
    private static string GetDigestHex(string algorithm, string key)
    {
        var keyBytes = Encoding.UTF8.GetBytes(key);


        switch (algorithm.ToLowerInvariant())
        {
            case "md5":
#pragma warning disable S4790 // Using weak hashing algorithms is security-sensitive
                var md5 = MD5.HashData(keyBytes);
                return BitConverter.ToString(md5).Replace("-", "").ToLowerInvariant();
#pragma warning restore S4790
            case "sha":
            case "sha1":
#pragma warning disable S4790 // Using weak hashing algorithms is security-sensitive
                var sha = SHA1.HashData(keyBytes);
                return BitConverter.ToString(sha).Replace("-", "").ToLowerInvariant();
#pragma warning restore S4790
            default:
                throw new AgenixSystemException("Unsupported digest algorithm: " + algorithm);
        }
    }

    /// <summary>
    ///     Sets the nonce validity period in milliseconds.
    /// </summary>
    /// <param name="nonceValidity">The nonce validity period to set</param>
    public void SetNonceValidity(long nonceValidity)
    {
        _nonceValidity = nonceValidity;
    }
}
