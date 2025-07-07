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

#endregion

using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using System.Text.Json;
using Agenix.Azure.Security.Client;
using Agenix.Azure.Security.Configuration;

namespace Agenix.Azure.Security.Tests.Client;

[TestFixture]
public class OAuthTokenClientTests
{
    private OAuthTokenClient _tokenClient;
    private TestHttpMessageHandler _testHttpHandler;
    private HttpClient _httpClient;
    private OAuthClientConfiguration _configuration;

    [SetUp]
    public void SetUp()
    {
        _testHttpHandler = new TestHttpMessageHandler();
        _httpClient = new HttpClient(_testHttpHandler);
        _tokenClient = new OAuthTokenClient(_httpClient);

        _configuration = new OAuthClientConfiguration
        {
            TokenEndpoint = "https://auth.example.com/token",
            ClientId = "test-client",
            ClientSecret = "test-secret",
            Scopes = new List<string> { "read", "write" }
        };
    }

    [TearDown]
    public void TearDown()
    {
        _tokenClient?.Dispose();
        _httpClient?.Dispose();
        _testHttpHandler?.Dispose();
    }

    [Test]
    public void Constructor_WithoutHttpClient_CreatesOwnHttpClient()
    {
        // Act
        using var client = new OAuthTokenClient();

        // Assert
        Assert.That(client, Is.Not.Null);
    }

    [Test]
    public void Constructor_WithHttpClient_UsesProvidedHttpClient()
    {
        // Arrange
        using var httpClient = new HttpClient();

        // Act
        using var client = new OAuthTokenClient(httpClient);

        // Assert
        Assert.That(client, Is.Not.Null);
    }

    [Test]
    public async Task GetTokenAsync_ValidConfiguration_ReturnsToken()
    {
        // Arrange
        var tokenResponse = new
        {
            access_token = "test-access-token",
            token_type = "Bearer",
            expires_in = 3600
        };

        _testHttpHandler.SetResponse(HttpStatusCode.OK, JsonSerializer.Serialize(tokenResponse));

        // Act
        var result = await _tokenClient.GetTokenAsync(_configuration);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.EqualTo("test-access-token"));
        Assert.That(_testHttpHandler.RequestCount, Is.EqualTo(1));
    }

    [Test]
    public async Task GetTokenAsync_CachedToken_ReturnsCachedToken()
    {
        // Arrange
        var validJwt = CreateTestJwtToken(DateTime.UtcNow.AddHours(1));
        var tokenResponse = new
        {
            access_token = validJwt,
            token_type = "Bearer",
            expires_in = 3600
        };

        _testHttpHandler.SetResponse(HttpStatusCode.OK, JsonSerializer.Serialize(tokenResponse));

        // Act
        var result1 = await _tokenClient.GetTokenAsync(_configuration);
        var result2 = await _tokenClient.GetTokenAsync(_configuration);

        // Assert
        Assert.That(result1, Is.EqualTo(validJwt));
        Assert.That(result2, Is.EqualTo(validJwt));
        Assert.That(_testHttpHandler.RequestCount, Is.EqualTo(1)); // Only one HTTP call should be made
    }


    [Test]
    public async Task GetTokenAsync_ExpiredCachedToken_AcquiresNewToken()
    {
        // Arrange
        var firstResponse = new
        {
            access_token = "first-token",
            token_type = "Bearer",
            expires_in = 3600
        };

        _testHttpHandler.SetResponse(HttpStatusCode.OK, JsonSerializer.Serialize(firstResponse));

        // Act
        var result1 = await _tokenClient.GetTokenAsync(_configuration, TimeSpan.FromMilliseconds(1));

        // Wait for cache to expire
        await Task.Delay(50);

        var secondResponse = new
        {
            access_token = "second-token",
            token_type = "Bearer",
            expires_in = 3600
        };
        _testHttpHandler.SetResponse(HttpStatusCode.OK, JsonSerializer.Serialize(secondResponse));

        var result2 = await _tokenClient.GetTokenAsync(_configuration, TimeSpan.FromMilliseconds(1));

        // Assert
        Assert.That(result1, Is.EqualTo("first-token"));
        Assert.That(result2, Is.EqualTo("second-token"));
        Assert.That(_testHttpHandler.RequestCount, Is.EqualTo(2));
    }

    [Test]
    public async Task GetTokenAsync_FailedTokenAcquisition_ReturnsNull()
    {
        // Arrange
        var errorResponse = new
        {
            error = "invalid_request",
            error_description = "Invalid request"
        };

        _testHttpHandler.SetResponse(HttpStatusCode.BadRequest, JsonSerializer.Serialize(errorResponse));

        // Act
        var result = await _tokenClient.GetTokenAsync(_configuration);

        // Assert
        Assert.That(result, Is.Null);
    }


    [Test]
    public async Task GetTokenAsync_InvalidConfiguration_ThrowsArgumentException()
    {
        // Arrange
        var invalidConfig = new OAuthClientConfiguration(); // Missing required fields

        // Act & Assert
        Assert.ThrowsAsync<ArgumentException>(() =>
            _tokenClient.GetTokenAsync(invalidConfig));
    }

    [Test]
    public async Task GetTokenAsync_CancellationRequested_ThrowsOperationCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();

        // Act & Assert
        Assert.ThrowsAsync<TaskCanceledException>(() =>
            _tokenClient.GetTokenAsync(_configuration, cancellationToken: cts.Token));
    }

    [Test]
    public async Task AcquireTokenAsync_SuccessfulResponse_ReturnsSuccessResult()
    {
        // Arrange
        var tokenResponse = new
        {
            access_token = "test-token",
            token_type = "Bearer",
            expires_in = 3600
        };

        _testHttpHandler.SetResponse(HttpStatusCode.OK, JsonSerializer.Serialize(tokenResponse));

        // Act
        var result = await _tokenClient.AcquireTokenAsync(_configuration);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.True);
        Assert.That(result.AccessToken, Is.EqualTo("test-token"));
        Assert.That(result.TokenType, Is.EqualTo("Bearer"));
    }

    [Test]
    public async Task AcquireTokenAsync_ClientError_DoesNotRetry()
    {
        // Arrange
        var errorResponse = new
        {
            error = "invalid_client",
            error_description = "Invalid client"
        };

        _testHttpHandler.SetResponse(HttpStatusCode.BadRequest, JsonSerializer.Serialize(errorResponse));

        // Act
        var result = await _tokenClient.AcquireTokenAsync(_configuration);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo("invalid_client"));
        Assert.That(_testHttpHandler.RequestCount, Is.EqualTo(1)); // Only one attempt should be made
    }

    [Test]
    public async Task AcquireTokenAsync_ServerError_RetriesWithBackoff()
    {
        // Arrange
        _configuration.RetryConfiguration.MaxRetries = 2;
        _configuration.RetryConfiguration.InitialDelay = TimeSpan.FromMilliseconds(10);
        _configuration.RetryConfiguration.UseJitter = false;

        var errorResponse = new
        {
            error = "server_error",
            error_description = "Server error"
        };

        _testHttpHandler.SetResponse(HttpStatusCode.InternalServerError, JsonSerializer.Serialize(errorResponse));

        // Act
        var result = await _tokenClient.AcquireTokenAsync(_configuration);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(_testHttpHandler.RequestCount, Is.EqualTo(3)); // Initial + 2 retries
    }

    [Test]
    public async Task AcquireTokenAsync_NetworkException_RetriesAndFails()
    {
        // Arrange
        _configuration.RetryConfiguration.MaxRetries = 1;
        _configuration.RetryConfiguration.InitialDelay = TimeSpan.FromMilliseconds(10);

        _testHttpHandler.SetException(new HttpRequestException("Network error"));

        // Act
        var result = await _tokenClient.AcquireTokenAsync(_configuration);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.IsSuccess, Is.False);
        Assert.That(result.Error, Is.EqualTo("request_failed"));
        Assert.That(_testHttpHandler.RequestCount, Is.EqualTo(2)); // Initial + 1 retry
    }

    [Test]
    public void ParseToken_ValidJwtToken_ReturnsJwtSecurityToken()
    {
        // Arrange
        var validJwt = CreateTestJwtToken();

        // Act
        var result = _tokenClient.ParseToken(validJwt);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.TypeOf<JwtSecurityToken>());
    }

    [Test]
    public void ParseToken_InvalidToken_ReturnsNull()
    {
        // Act
        var result = _tokenClient.ParseToken("invalid-token");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void ParseToken_NullOrEmptyToken_ReturnsNull()
    {
        // Act & Assert
        Assert.That(_tokenClient.ParseToken(null), Is.Null);
        Assert.That(_tokenClient.ParseToken(""), Is.Null);
        Assert.That(_tokenClient.ParseToken("   "), Is.Null);
    }

    [Test]
    public void IsTokenExpired_ExpiredToken_ReturnsTrue()
    {
        // Arrange
        var expiredJwt = CreateTestJwtToken(DateTime.UtcNow.AddMinutes(-10));

        // Act
        var result = _tokenClient.IsTokenExpired(expiredJwt);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsTokenExpired_ValidToken_ReturnsFalse()
    {
        // Arrange
        var validJwt = CreateTestJwtToken(DateTime.UtcNow.AddHours(1));

        // Act
        var result = _tokenClient.IsTokenExpired(validJwt);

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void IsTokenExpired_TokenExpiringWithinBuffer_ReturnsTrue()
    {
        // Arrange
        var soonExpiringJwt = CreateTestJwtToken(DateTime.UtcNow.AddMinutes(2));
        var buffer = TimeSpan.FromMinutes(5);

        // Act
        var result = _tokenClient.IsTokenExpired(soonExpiringJwt, buffer);

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void IsTokenExpired_NullOrInvalidToken_ReturnsTrue()
    {
        // Act & Assert
        Assert.That(_tokenClient.IsTokenExpired(null), Is.True);
        Assert.That(_tokenClient.IsTokenExpired(""), Is.True);
        Assert.That(_tokenClient.IsTokenExpired("invalid-token"), Is.True);
    }

    [Test]
    public void GetTokenExpiration_ValidToken_ReturnsExpiration()
    {
        // Arrange
        var expiration = DateTime.UtcNow.AddHours(1);
        var jwt = CreateTestJwtToken(expiration);

        // Act
        var result = _tokenClient.GetTokenExpiration(jwt);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value, Is.EqualTo(expiration).Within(TimeSpan.FromSeconds(1)));
    }

    [Test]
    public void GetTokenExpiration_InvalidToken_ReturnsNull()
    {
        // Act
        var result = _tokenClient.GetTokenExpiration("invalid-token");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public void GetRemainingTokenLifetime_ValidToken_ReturnsRemainingTime()
    {
        // Arrange
        var expiration = DateTime.UtcNow.AddHours(1);
        var jwt = CreateTestJwtToken(expiration);

        // Act
        var result = _tokenClient.GetRemainingTokenLifetime(jwt);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value, Is.GreaterThan(TimeSpan.FromMinutes(59)));
        Assert.That(result.Value, Is.LessThanOrEqualTo(TimeSpan.FromHours(1)));
    }

    [Test]
    public void GetRemainingTokenLifetime_ExpiredToken_ReturnsZero()
    {
        // Arrange
        var expiration = DateTime.UtcNow.AddMinutes(-10);
        var jwt = CreateTestJwtToken(expiration);

        // Act
        var result = _tokenClient.GetRemainingTokenLifetime(jwt);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Value, Is.EqualTo(TimeSpan.Zero));
    }

    [Test]
    public void GetRemainingTokenLifetime_InvalidToken_ReturnsNull()
    {
        // Act
        var result = _tokenClient.GetRemainingTokenLifetime("invalid-token");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task RemoveToken_ExistingCacheKey_RemovesTokenFromCache()
    {
        // Arrange
        var tokenResponse = new
        {
            access_token = "test-token",
            token_type = "Bearer",
            expires_in = 3600
        };

        _testHttpHandler.SetResponse(HttpStatusCode.OK, JsonSerializer.Serialize(tokenResponse));

        // Get token to cache it
        await _tokenClient.GetTokenAsync(_configuration);
        var cacheKey = _configuration.GetCacheKey();
        var initialRequestCount = _testHttpHandler.RequestCount;

        // Act
        _tokenClient.RemoveToken(cacheKey);

        // Setup second response for verification
        var secondResponse = new
        {
            access_token = "new-token",
            token_type = "Bearer",
            expires_in = 3600
        };
        _testHttpHandler.SetResponse(HttpStatusCode.OK, JsonSerializer.Serialize(secondResponse));

        var result = await _tokenClient.GetTokenAsync(_configuration);

        // Assert
        Assert.That(result, Is.EqualTo("new-token"));
        Assert.That(_testHttpHandler.RequestCount, Is.EqualTo(initialRequestCount + 1)); // One more call after cache clear
    }

    [Test]
    public async Task ClearCache_WithCachedTokens_ClearsAllTokens()
    {
        // Arrange
        var tokenResponse = new
        {
            access_token = "test-token",
            token_type = "Bearer",
            expires_in = 3600
        };

        _testHttpHandler.SetResponse(HttpStatusCode.OK, JsonSerializer.Serialize(tokenResponse));

        // Get token to cache it
        await _tokenClient.GetTokenAsync(_configuration);
        var initialRequestCount = _testHttpHandler.RequestCount;

        // Act
        _tokenClient.ClearCache();

        // Setup second response for verification
        var secondResponse = new
        {
            access_token = "new-token",
            token_type = "Bearer",
            expires_in = 3600
        };
        _testHttpHandler.SetResponse(HttpStatusCode.OK, JsonSerializer.Serialize(secondResponse));

        var result = await _tokenClient.GetTokenAsync(_configuration);

        // Assert
        Assert.That(result, Is.EqualTo("new-token"));
        Assert.That(_testHttpHandler.RequestCount, Is.EqualTo(initialRequestCount + 1)); // One more call after cache clear
    }

    [Test]
    public async Task GetCacheStatistics_WithCachedTokens_ReturnsCorrectStatistics()
    {
        // Arrange
        var tokenResponse = new
        {
            access_token = "test-token",
            token_type = "Bearer",
            expires_in = 3600
        };

        _testHttpHandler.SetResponse(HttpStatusCode.OK, JsonSerializer.Serialize(tokenResponse));

        // Get token to cache it
        await _tokenClient.GetTokenAsync(_configuration);

        // Act
        var stats = _tokenClient.GetCacheStatistics();

        // Assert
        Assert.That(stats, Is.Not.Null);
        Assert.That(stats.TotalTokens, Is.EqualTo(1));
        Assert.That(stats.ValidTokens, Is.EqualTo(1));
        Assert.That(stats.ExpiredTokens, Is.EqualTo(0));
        Assert.That(stats.OldestToken, Is.Not.Null);
        Assert.That(stats.NewestToken, Is.Not.Null);
    }

    [Test]
    public void GetCacheStatistics_EmptyCache_ReturnsZeroStatistics()
    {
        // Act
        var stats = _tokenClient.GetCacheStatistics();

        // Assert
        Assert.That(stats, Is.Not.Null);
        Assert.That(stats.TotalTokens, Is.EqualTo(0));
        Assert.That(stats.ValidTokens, Is.EqualTo(0));
        Assert.That(stats.ExpiredTokens, Is.EqualTo(0));
        Assert.That(stats.OldestToken, Is.Null);
        Assert.That(stats.NewestToken, Is.Null);
    }

    [Test]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Act & Assert
        Assert.DoesNotThrow(() =>
        {
            _tokenClient.Dispose();
            _tokenClient.Dispose();
        });
    }

    [Test]
    public async Task GetTokenAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        _tokenClient.Dispose();

        // Act & Assert
        Assert.ThrowsAsync<ObjectDisposedException>(() =>
            _tokenClient.GetTokenAsync(_configuration));
    }

    [Test]
    public async Task AcquireTokenAsync_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        _tokenClient.Dispose();

        // Act & Assert
        Assert.ThrowsAsync<ObjectDisposedException>(() =>
            _tokenClient.AcquireTokenAsync(_configuration));
    }

    private string CreateTestJwtToken(DateTime? expiration = null)
    {
        var exp = expiration ?? DateTime.UtcNow.AddHours(1);
        var unixExpiration = ((DateTimeOffset)exp).ToUnixTimeSeconds();

        var header = Base64UrlEncode(JsonSerializer.Serialize(new { alg = "HS256", typ = "JWT" }));
        var payload = Base64UrlEncode(JsonSerializer.Serialize(new { exp = unixExpiration }));
        var signature = Base64UrlEncode("test-signature");

        return $"{header}.{payload}.{signature}";
    }

    private static string Base64UrlEncode(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        return Convert.ToBase64String(bytes)
            .Replace('+', '-')
            .Replace('/', '_')
            .TrimEnd('=');
    }


    // Test HTTP Message Handler to track requests
    private class TestHttpMessageHandler : HttpMessageHandler
    {
        private HttpStatusCode _statusCode = HttpStatusCode.OK;
        private string _content = string.Empty;
        private Exception? _exception;

        public int RequestCount { get; private set; }
        public List<HttpRequestMessage> Requests { get; } = new();

        public void SetResponse(HttpStatusCode statusCode, string content)
        {
            _statusCode = statusCode;
            _content = content;
            _exception = null;
        }

        public void SetException(Exception exception)
        {
            _exception = exception;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            RequestCount++;
            Requests.Add(request);

            if (_exception != null)
            {
                throw _exception;
            }

            await Task.Delay(1, cancellationToken); // Simulate network delay

            return new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_content, Encoding.UTF8, "application/json")
            };
        }
    }
}
