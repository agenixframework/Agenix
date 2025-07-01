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

namespace Agenix.Azure.Security.Tests.Configuration;

[TestFixture]
public class OAuthClientConfigurationTests
{
    private OAuthClientConfiguration _configuration;

    [SetUp]
    public void SetUp()
    {
        _configuration = new OAuthClientConfiguration();
    }

    [Test]
    public void Constructor_DefaultValues_AreSetCorrectly()
    {
        // Assert
        Assert.That(_configuration.TokenEndpoint, Is.EqualTo(string.Empty));
        Assert.That(_configuration.ClientId, Is.EqualTo(string.Empty));
        Assert.That(_configuration.ClientSecret, Is.EqualTo(string.Empty));
        Assert.That(_configuration.Scopes, Is.Not.Null);
        Assert.That(_configuration.Scopes, Is.Empty);
        Assert.That(_configuration.AdditionalParameters, Is.Not.Null);
        Assert.That(_configuration.AdditionalParameters, Is.Empty);
        Assert.That(_configuration.CustomHeaders, Is.Not.Null);
        Assert.That(_configuration.CustomHeaders, Is.Empty);
        Assert.That(_configuration.Timeout, Is.EqualTo(TimeSpan.FromSeconds(30)));
        Assert.That(_configuration.RetryConfiguration, Is.Not.Null);
    }

    [Test]
    public void GetFormData_BasicConfiguration_ReturnsCorrectFormData()
    {
        // Arrange
        _configuration.ClientId = "test-client";
        _configuration.ClientSecret = "test-secret";

        // Act
        var formData = _configuration.GetFormData();

        // Assert
        Assert.That(formData, Is.Not.Null);
        Assert.That(formData, Contains.Key("grant_type"));
        Assert.That(formData["grant_type"], Is.EqualTo("client_credentials"));
        Assert.That(formData, Contains.Key("client_id"));
        Assert.That(formData["client_id"], Is.EqualTo("test-client"));
        Assert.That(formData, Contains.Key("client_secret"));
        Assert.That(formData["client_secret"], Is.EqualTo("test-secret"));
    }

    [Test]
    public void GetFormData_WithScopes_IncludesScopeInFormData()
    {
        // Arrange
        _configuration.ClientId = "test-client";
        _configuration.ClientSecret = "test-secret";
        _configuration.Scopes.AddRange(new[] { "read", "write", "admin" });

        // Act
        var formData = _configuration.GetFormData();

        // Assert
        Assert.That(formData, Contains.Key("scope"));
        Assert.That(formData["scope"], Is.EqualTo("read write admin"));
    }

    [Test]
    public void GetFormData_WithAdditionalParameters_IncludesParametersInFormData()
    {
        // Arrange
        _configuration.ClientId = "test-client";
        _configuration.ClientSecret = "test-secret";
        _configuration.AdditionalParameters["audience"] = "https://api.example.com";
        _configuration.AdditionalParameters["resource"] = "urn:resource:identifier";

        // Act
        var formData = _configuration.GetFormData();

        // Assert
        Assert.That(formData, Contains.Key("audience"));
        Assert.That(formData["audience"], Is.EqualTo("https://api.example.com"));
        Assert.That(formData, Contains.Key("resource"));
        Assert.That(formData["resource"], Is.EqualTo("urn:resource:identifier"));
    }

    [Test]
    public void GetFormData_WithScopesAndAdditionalParameters_IncludesAllData()
    {
        // Arrange
        _configuration.ClientId = "test-client";
        _configuration.ClientSecret = "test-secret";
        _configuration.Scopes.Add("read");
        _configuration.AdditionalParameters["audience"] = "api";

        // Act
        var formData = _configuration.GetFormData();

        // Assert
        Assert.That(formData.Count, Is.EqualTo(5)); // grant_type, client_id, client_secret, scope, audience
        Assert.That(formData["scope"], Is.EqualTo("read"));
        Assert.That(formData["audience"], Is.EqualTo("api"));
    }

    [Test]
    public void GetCacheKey_BasicConfiguration_ReturnsHashedKey()
    {
        // Arrange
        _configuration.TokenEndpoint = "https://auth.example.com/token";
        _configuration.ClientId = "test-client";

        // Act
        var cacheKey = _configuration.GetCacheKey();

        // Assert
        Assert.That(cacheKey, Is.Not.Null);
        Assert.That(cacheKey, Is.Not.Empty);
        Assert.That(cacheKey, Does.Not.Contain("/"));
        Assert.That(cacheKey, Does.Not.Contain("+"));
        Assert.That(cacheKey, Does.Not.EndWith("="));
    }

    [Test]
    public void GetCacheKey_WithScopes_IncludesScopesInKey()
    {
        // Arrange
        _configuration.TokenEndpoint = "https://auth.example.com/token";
        _configuration.ClientId = "test-client";
        _configuration.Scopes.AddRange(new[] { "write", "read" }); // Note: different order

        var configuration2 = new OAuthClientConfiguration
        {
            TokenEndpoint = "https://auth.example.com/token",
            ClientId = "test-client"
        };
        configuration2.Scopes.AddRange(new[] { "read", "write" }); // Same scopes, different order

        // Act
        var cacheKey1 = _configuration.GetCacheKey();
        var cacheKey2 = configuration2.GetCacheKey();

        // Assert
        Assert.That(cacheKey1, Is.EqualTo(cacheKey2)); // Should be same because scopes are ordered
    }

    [Test]
    public void GetCacheKey_WithAdditionalParameters_IncludesParametersInKey()
    {
        // Arrange
        _configuration.TokenEndpoint = "https://auth.example.com/token";
        _configuration.ClientId = "test-client";
        _configuration.AdditionalParameters["param1"] = "value1";
        _configuration.AdditionalParameters["param2"] = "value2";

        // Act
        var cacheKey = _configuration.GetCacheKey();

        // Assert
        Assert.That(cacheKey, Is.Not.Null);
        Assert.That(cacheKey, Is.Not.Empty);
    }

    [Test]
    public void GetCacheKey_DifferentConfigurations_ReturnDifferentKeys()
    {
        // Arrange
        var config1 = new OAuthClientConfiguration
        {
            TokenEndpoint = "https://auth1.example.com/token",
            ClientId = "client1"
        };

        var config2 = new OAuthClientConfiguration
        {
            TokenEndpoint = "https://auth2.example.com/token",
            ClientId = "client2"
        };

        // Act
        var key1 = config1.GetCacheKey();
        var key2 = config2.GetCacheKey();

        // Assert
        Assert.That(key1, Is.Not.EqualTo(key2));
    }

    [Test]
    public void GetCacheKey_SameConfigurations_ReturnSameKeys()
    {
        // Arrange
        var config1 = new OAuthClientConfiguration
        {
            TokenEndpoint = "https://auth.example.com/token",
            ClientId = "test-client"
        };
        config1.Scopes.Add("read");

        var config2 = new OAuthClientConfiguration
        {
            TokenEndpoint = "https://auth.example.com/token",
            ClientId = "test-client"
        };
        config2.Scopes.Add("read");

        // Act
        var key1 = config1.GetCacheKey();
        var key2 = config2.GetCacheKey();

        // Assert
        Assert.That(key1, Is.EqualTo(key2));
    }

    [Test]
    public void Validate_ValidConfiguration_DoesNotThrow()
    {
        // Arrange
        _configuration.TokenEndpoint = "https://auth.example.com/token";
        _configuration.ClientId = "test-client";
        _configuration.ClientSecret = "test-secret";

        // Act & Assert
        Assert.DoesNotThrow(() => _configuration.Validate());
    }

    [Test]
    public void Validate_NullTokenEndpoint_ThrowsArgumentException()
    {
        // Arrange
        _configuration.TokenEndpoint = null;
        _configuration.ClientId = "test-client";
        _configuration.ClientSecret = "test-secret";

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _configuration.Validate());
        Assert.That(ex.ParamName, Is.EqualTo("TokenEndpoint"));
        Assert.That(ex.Message, Does.Contain("TokenEndpoint is required"));
    }

    [Test]
    public void Validate_EmptyTokenEndpoint_ThrowsArgumentException()
    {
        // Arrange
        _configuration.TokenEndpoint = "";
        _configuration.ClientId = "test-client";
        _configuration.ClientSecret = "test-secret";

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _configuration.Validate());
        Assert.That(ex.ParamName, Is.EqualTo("TokenEndpoint"));
    }

    [Test]
    public void Validate_WhitespaceTokenEndpoint_ThrowsArgumentException()
    {
        // Arrange
        _configuration.TokenEndpoint = "   ";
        _configuration.ClientId = "test-client";
        _configuration.ClientSecret = "test-secret";

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _configuration.Validate());
        Assert.That(ex.ParamName, Is.EqualTo("TokenEndpoint"));
    }

    [Test]
    public void Validate_NullClientId_ThrowsArgumentException()
    {
        // Arrange
        _configuration.TokenEndpoint = "https://auth.example.com/token";
        _configuration.ClientId = null;
        _configuration.ClientSecret = "test-secret";

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _configuration.Validate());
        Assert.That(ex.ParamName, Is.EqualTo("ClientId"));
        Assert.That(ex.Message, Does.Contain("ClientId is required"));
    }

    [Test]
    public void Validate_EmptyClientId_ThrowsArgumentException()
    {
        // Arrange
        _configuration.TokenEndpoint = "https://auth.example.com/token";
        _configuration.ClientId = "";
        _configuration.ClientSecret = "test-secret";

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _configuration.Validate());
        Assert.That(ex.ParamName, Is.EqualTo("ClientId"));
    }

    [Test]
    public void Validate_NullClientSecret_ThrowsArgumentException()
    {
        // Arrange
        _configuration.TokenEndpoint = "https://auth.example.com/token";
        _configuration.ClientId = "test-client";
        _configuration.ClientSecret = null;

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _configuration.Validate());
        Assert.That(ex.ParamName, Is.EqualTo("ClientSecret"));
        Assert.That(ex.Message, Does.Contain("ClientSecret is required"));
    }

    [Test]
    public void Validate_EmptyClientSecret_ThrowsArgumentException()
    {
        // Arrange
        _configuration.TokenEndpoint = "https://auth.example.com/token";
        _configuration.ClientId = "test-client";
        _configuration.ClientSecret = "";

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _configuration.Validate());
        Assert.That(ex.ParamName, Is.EqualTo("ClientSecret"));
    }

    [Test]
    public void Validate_InvalidTokenEndpointUrl_ThrowsArgumentException()
    {
        // Arrange
        _configuration.TokenEndpoint = "not-a-valid-url";
        _configuration.ClientId = "test-client";
        _configuration.ClientSecret = "test-secret";

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _configuration.Validate());
        Assert.That(ex.ParamName, Is.EqualTo("TokenEndpoint"));
        Assert.That(ex.Message, Does.Contain("must be a valid absolute URL"));
    }

    [Test]
    public void Validate_RelativeTokenEndpointUrl_ThrowsArgumentException()
    {
        // Arrange
        _configuration.TokenEndpoint = "/oauth/token";
        _configuration.ClientId = "test-client";
        _configuration.ClientSecret = "test-secret";

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _configuration.Validate());
        Assert.Multiple(() =>
        {
            Assert.That(ex.ParamName, Is.EqualTo("TokenEndpoint"));
            Assert.That(ex.Message, Does.Match(@"TokenEndpoint must (use HTTP or HTTPS scheme|be a valid absolute URL)"));
        });
    }

    [Test]
    public void Validate_InvalidScheme_ThrowsArgumentException()
    {
        // Arrange
        _configuration.TokenEndpoint = "ftp://auth.example.com/token";
        _configuration.ClientId = "test-client";
        _configuration.ClientSecret = "test-secret";

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _configuration.Validate());
        Assert.That(ex.ParamName, Is.EqualTo("TokenEndpoint"));
        Assert.That(ex.Message, Does.Contain("must use HTTP or HTTPS scheme"));
    }

    [Test]
    public void Validate_HttpScheme_DoesNotThrow()
    {
        // Arrange
        _configuration.TokenEndpoint = "http://auth.example.com/token";
        _configuration.ClientId = "test-client";
        _configuration.ClientSecret = "test-secret";

        // Act & Assert
        Assert.DoesNotThrow(() => _configuration.Validate());
    }

    [Test]
    public void Validate_HttpsScheme_DoesNotThrow()
    {
        // Arrange
        _configuration.TokenEndpoint = "https://auth.example.com/token";
        _configuration.ClientId = "test-client";
        _configuration.ClientSecret = "test-secret";

        // Act & Assert
        Assert.DoesNotThrow(() => _configuration.Validate());
    }

    [Test]
    public void Validate_ZeroTimeout_ThrowsArgumentException()
    {
        // Arrange
        _configuration.TokenEndpoint = "https://auth.example.com/token";
        _configuration.ClientId = "test-client";
        _configuration.ClientSecret = "test-secret";
        _configuration.Timeout = TimeSpan.Zero;

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _configuration.Validate());
        Assert.That(ex.ParamName, Is.EqualTo("Timeout"));
        Assert.That(ex.Message, Does.Contain("Timeout must be positive"));
    }

    [Test]
    public void Validate_NegativeTimeout_ThrowsArgumentException()
    {
        // Arrange
        _configuration.TokenEndpoint = "https://auth.example.com/token";
        _configuration.ClientId = "test-client";
        _configuration.ClientSecret = "test-secret";
        _configuration.Timeout = TimeSpan.FromSeconds(-1);

        // Act & Assert
        var ex = Assert.Throws<ArgumentException>(() => _configuration.Validate());
        Assert.That(ex.ParamName, Is.EqualTo("Timeout"));
        Assert.That(ex.Message, Does.Contain("Timeout must be positive"));
    }

    [Test]
    public void Properties_CanBeSetAndRetrieved()
    {
        // Arrange & Act
        _configuration.TokenEndpoint = "https://auth.example.com/token";
        _configuration.ClientId = "test-client";
        _configuration.ClientSecret = "test-secret";
        _configuration.Scopes.Add("read");
        _configuration.AdditionalParameters["audience"] = "api";
        _configuration.CustomHeaders["X-Custom"] = "value";
        _configuration.Timeout = TimeSpan.FromMinutes(5);

        // Assert
        Assert.That(_configuration.TokenEndpoint, Is.EqualTo("https://auth.example.com/token"));
        Assert.That(_configuration.ClientId, Is.EqualTo("test-client"));
        Assert.That(_configuration.ClientSecret, Is.EqualTo("test-secret"));
        Assert.That(_configuration.Scopes, Contains.Item("read"));
        Assert.That(_configuration.AdditionalParameters["audience"], Is.EqualTo("api"));
        Assert.That(_configuration.CustomHeaders["X-Custom"], Is.EqualTo("value"));
        Assert.That(_configuration.Timeout, Is.EqualTo(TimeSpan.FromMinutes(5)));
    }
}
