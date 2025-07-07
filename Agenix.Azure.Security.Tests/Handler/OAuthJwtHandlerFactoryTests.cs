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

using Agenix.Azure.Security.Configuration;
using Agenix.Azure.Security.Handler;

namespace Agenix.Azure.Security.Tests.Handler;

[TestFixture]
public class OAuthJwtHandlerFactoryTests
{
    private const string TestTokenEndpoint = "https://auth.example.com/oauth/token";
    private const string TestClientId = "test-client-id";
    private const string TestClientSecret = "test-client-secret";
    private const string TestScope = "read";

    [Test]
    public void CreateClientCredentialsHandler_ValidParameters_ReturnsHandler()
    {
        // Act
        var handler = OAuthJwtHandlerFactory.CreateClientCredentialsHandler(
            TestTokenEndpoint,
            TestClientId,
            TestClientSecret);

        // Assert
        Assert.That(handler, Is.Not.Null);
        Assert.That(handler, Is.TypeOf<OAuthJwtAuthenticationHandler>());
    }

    [Test]
    public void CreateClientCredentialsHandler_WithScopes_ReturnsHandlerWithScopes()
    {
        // Arrange
        var scopes = new[] { "read", "write", "admin" };

        // Act
        var handler = OAuthJwtHandlerFactory.CreateClientCredentialsHandler(
            TestTokenEndpoint,
            TestClientId,
            TestClientSecret,
            scopes);

        // Assert
        Assert.That(handler, Is.Not.Null);
        Assert.That(handler, Is.TypeOf<OAuthJwtAuthenticationHandler>());
    }

    [Test]
    public void CreateClientCredentialsHandler_WithOptions_ReturnsHandlerWithOptions()
    {
        // Arrange
        var options = new OAuthJwtHandlerOptions
        {
            AuthenticationScheme = "https://auth.example.com",
            RetryOnUnauthorized = false
        };

        // Act
        var handler = OAuthJwtHandlerFactory.CreateClientCredentialsHandler(
            TestTokenEndpoint,
            TestClientId,
            TestClientSecret,
            options: options);

        // Assert
        Assert.That(handler, Is.Not.Null);
        Assert.That(handler, Is.TypeOf<OAuthJwtAuthenticationHandler>());
    }

    [Test]
    public void CreateClientCredentialsHandler_NullTokenEndpoint_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            OAuthJwtHandlerFactory.CreateClientCredentialsHandler(null, TestClientId, TestClientSecret));
    }

    [Test]
    public void CreateClientCredentialsHandler_NullClientId_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            OAuthJwtHandlerFactory.CreateClientCredentialsHandler(TestTokenEndpoint, null, TestClientSecret));
    }

    [Test]
    public void CreateClientCredentialsHandler_NullClientSecret_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            OAuthJwtHandlerFactory.CreateClientCredentialsHandler(TestTokenEndpoint, TestClientId, null));
    }

    [Test]
    public void CreateHandler_WithOAuthConfiguration_ReturnsHandler()
    {
        // Arrange
        var config = new OAuthClientConfiguration
        {
            TokenEndpoint = TestTokenEndpoint,
            ClientId = TestClientId,
            ClientSecret = TestClientSecret,
            Scopes = [TestScope]
        };

        // Act
        var handler = OAuthJwtHandlerFactory.CreateHandler(config);

        // Assert
        Assert.That(handler, Is.Not.Null);
        Assert.That(handler, Is.TypeOf<OAuthJwtAuthenticationHandler>());
    }

    [Test]
    public void CreateHandler_WithConfigurationAndOptions_ReturnsHandler()
    {
        // Arrange
        var config = new OAuthClientConfiguration
        {
            TokenEndpoint = TestTokenEndpoint,
            ClientId = TestClientId,
            ClientSecret = TestClientSecret
        };
        var options = new OAuthJwtHandlerOptions { OverrideExistingAuthHeader = false };

        // Act
        var handler = OAuthJwtHandlerFactory.CreateHandler(config, options);

        // Assert
        Assert.That(handler, Is.Not.Null);
        Assert.That(handler, Is.TypeOf<OAuthJwtAuthenticationHandler>());
    }

    [Test]
    public void CreateHandler_WithHttpClient_ReturnsHandler()
    {
        // Arrange
        var config = new OAuthClientConfiguration
        {
            TokenEndpoint = TestTokenEndpoint,
            ClientId = TestClientId,
            ClientSecret = TestClientSecret
        };
        var httpClient = new HttpClient();

        try
        {
            // Act
            var handler = OAuthJwtHandlerFactory.CreateHandler(config, httpClient);

            // Assert
            Assert.That(handler, Is.Not.Null);
            Assert.That(handler, Is.TypeOf<OAuthJwtAuthenticationHandler>());
        }
        finally
        {
            httpClient.Dispose();
        }
    }

    [Test]
    public void CreateHandler_NullConfiguration_ThrowsArgumentNullException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() =>
            OAuthJwtHandlerFactory.CreateHandler(null));
    }

    [Test]
    public void CreateForScope_ValidParameters_ReturnsHandler()
    {
        // Act
        var handler = OAuthJwtHandlerFactory.CreateForScope(
            TestTokenEndpoint,
            TestClientId,
            TestClientSecret,
            TestScope);

        // Assert
        Assert.That(handler, Is.Not.Null);
        Assert.That(handler, Is.TypeOf<OAuthJwtAuthenticationHandler>());
    }

    [Test]
    public void CreateForScope_WithOptions_ReturnsHandler()
    {
        // Arrange
        var options = new OAuthJwtHandlerOptions { FailOnTokenAcquisitionError = false };

        // Act
        var handler = OAuthJwtHandlerFactory.CreateForScope(
            TestTokenEndpoint,
            TestClientId,
            TestClientSecret,
            TestScope,
            options);

        // Assert
        Assert.That(handler, Is.Not.Null);
        Assert.That(handler, Is.TypeOf<OAuthJwtAuthenticationHandler>());
    }

    [Test]
    public void CreateWithProviders_ValidProviders_ReturnsHandler()
    {
        // Arrange
        Func<string> clientIdProvider = () => TestClientId;
        Func<string> clientSecretProvider = () => TestClientSecret;

        // Act
        var handler = OAuthJwtHandlerFactory.CreateWithProviders(
            TestTokenEndpoint,
            clientIdProvider,
            clientSecretProvider,
            TestScope);

        // Assert
        Assert.That(handler, Is.Not.Null);
        Assert.That(handler, Is.TypeOf<OAuthJwtAuthenticationHandler>());
    }

    [Test]
    public void CreateWithConfiguration_ValidConfigurationProvider_ReturnsHandler()
    {
        // Arrange
        Func<OAuthClientConfiguration> configProvider = () => new OAuthClientConfiguration
        {
            TokenEndpoint = TestTokenEndpoint,
            ClientId = TestClientId,
            ClientSecret = TestClientSecret,
            Scopes = [TestScope]
        };

        // Act
        var handler = OAuthJwtHandlerFactory.CreateWithConfiguration(configProvider);

        // Assert
        Assert.That(handler, Is.Not.Null);
        Assert.That(handler, Is.TypeOf<OAuthJwtAuthenticationHandler>());
    }

    [Test]
    public void CreateWithParameters_ValidParameters_ReturnsHandler()
    {
        // Arrange
        var scopes = new[] { "read", "write" };
        var additionalParams = new Dictionary<string, string> { { "audience", "api" } };
        var customHeaders = new Dictionary<string, string> { { "X-Custom", "value" } };

        // Act
        var handler = OAuthJwtHandlerFactory.CreateWithParameters(
            TestTokenEndpoint,
            TestClientId,
            TestClientSecret,
            scopes,
            additionalParams,
            customHeaders);

        // Assert
        Assert.That(handler, Is.Not.Null);
        Assert.That(handler, Is.TypeOf<OAuthJwtAuthenticationHandler>());
    }

    [Test]
    public void CreateWithParameters_NullOptionalParameters_ReturnsHandler()
    {
        // Act
        var handler = OAuthJwtHandlerFactory.CreateWithParameters(
            TestTokenEndpoint,
            TestClientId,
            TestClientSecret,
            scopes: null,
            additionalParameters: null,
            customHeaders: null);

        // Assert
        Assert.That(handler, Is.Not.Null);
        Assert.That(handler, Is.TypeOf<OAuthJwtAuthenticationHandler>());
    }

    [Test]
    public void CreateWithRetry_DefaultRetrySettings_ReturnsHandler()
    {
        // Act
        var handler = OAuthJwtHandlerFactory.CreateWithRetry(
            TestTokenEndpoint,
            TestClientId,
            TestClientSecret);

        // Assert
        Assert.That(handler, Is.Not.Null);
        Assert.That(handler, Is.TypeOf<OAuthJwtAuthenticationHandler>());
    }

    [Test]
    public void CreateWithRetry_CustomRetrySettings_ReturnsHandler()
    {
        // Arrange
        var scopes = new[] { TestScope };
        var maxRetries = 5;
        var initialDelay = TimeSpan.FromSeconds(2);

        // Act
        var handler = OAuthJwtHandlerFactory.CreateWithRetry(
            TestTokenEndpoint,
            TestClientId,
            TestClientSecret,
            scopes,
            maxRetries,
            initialDelay);

        // Assert
        Assert.That(handler, Is.Not.Null);
        Assert.That(handler, Is.TypeOf<OAuthJwtAuthenticationHandler>());
    }

    [Test]
    public void MultipleFactoryMethods_ReturnDifferentInstances()
    {
        // Act
        var handler1 = OAuthJwtHandlerFactory.CreateClientCredentialsHandler(
            TestTokenEndpoint, TestClientId, TestClientSecret);
        var handler2 = OAuthJwtHandlerFactory.CreateForScope(
            TestTokenEndpoint, TestClientId, TestClientSecret, TestScope);

        // Assert
        Assert.That(handler1, Is.Not.Null);
        Assert.That(handler2, Is.Not.Null);
        Assert.That(handler1, Is.Not.SameAs(handler2));
    }

    [Test]
    public void FactoryMethods_WithEmptyStringParameters_ThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            OAuthJwtHandlerFactory.CreateClientCredentialsHandler("", TestClientId, TestClientSecret));

        Assert.Throws<ArgumentException>(() =>
            OAuthJwtHandlerFactory.CreateClientCredentialsHandler(TestTokenEndpoint, "", TestClientSecret));

        Assert.Throws<ArgumentException>(() =>
            OAuthJwtHandlerFactory.CreateClientCredentialsHandler(TestTokenEndpoint, TestClientId, ""));
    }

    [Test]
    public void FactoryMethods_WithWhitespaceParameters_ThrowArgumentException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            OAuthJwtHandlerFactory.CreateClientCredentialsHandler("   ", TestClientId, TestClientSecret));

        Assert.Throws<ArgumentException>(() =>
            OAuthJwtHandlerFactory.CreateClientCredentialsHandler(TestTokenEndpoint, "   ", TestClientSecret));

        Assert.Throws<ArgumentException>(() =>
            OAuthJwtHandlerFactory.CreateClientCredentialsHandler(TestTokenEndpoint, TestClientId, "   "));
    }
}
