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

using System.Net;
using System.Reflection;
using System.Text;
using Agenix.Api.Report;
using Agenix.Core.Message;
using Agenix.GraphQL.Interceptor;
using Moq;
using Moq.Protected;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.GraphQL.Tests.Interceptor;

[TestFixture]
public class LoggingGraphQLClientHandlerTest
{
    [SetUp]
    public void SetUp()
    {
        _mockInnerHandler = new Mock<HttpMessageHandler>();
        _handler = new LoggingGraphQlClientHandler(_mockInnerHandler.Object);
        _testableHandler = new TestableLoggingGraphQlClientHandler(_mockInnerHandler.Object);
        _mockMessageListeners = new Mock<AsyncMessageListeners>();

        // Setup message listeners to return false for IsEmpty by default
        _mockMessageListeners.Setup(ml => ml.IsEmpty()).Returns(false);
    }

    [TearDown]
    public void TearDown()
    {
        _handler?.Dispose();
        _testableHandler?.Dispose();
    }

    private Mock<HttpMessageHandler> _mockInnerHandler;
    private LoggingGraphQlClientHandler _handler;
    private Mock<AsyncMessageListeners> _mockMessageListeners;
    private TestableLoggingGraphQlClientHandler _testableHandler;

    // Testable wrapper that exposes the protected SendAsync method
    private class TestableLoggingGraphQlClientHandler(HttpMessageHandler innerHandler)
        : LoggingGraphQlClientHandler(innerHandler)
    {
        public new Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return base.SendAsync(request, cancellationToken);
        }
    }

    [Test]
    public async Task TestSendGraphQlQueryRequest()
    {
        // Arrange
        const string graphqlQuery = """
                                    {
                                        "query": "query GetUser($id: ID!) { user(id: $id) { id name email } }",
                                        "variables": {"id": "123"},
                                        "operationName": "GetUser"
                                    }
                                    """;

        var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/graphql")
        {
            Content = new StringContent(graphqlQuery, Encoding.UTF8, "application/json")
        };

        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""
                                        {
                                            "data": {
                                                "user": {
                                                    "id": "123",
                                                    "name": "John Doe",
                                                    "email": "john@example.com"
                                                }
                                            }
                                        }
                                        """, Encoding.UTF8, "application/json")
        };

        _mockInnerHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);

        // Act
        var actualResponse = await _testableHandler.SendAsync(request, CancellationToken.None);

        // Assert
        Assert.That(actualResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        _mockInnerHandler.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
    }

    [Test]
    public async Task TestSendRequestWithoutContent()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/graphql");

        var expectedResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Bad Request", Encoding.UTF8, "text/plain")
        };

        _mockInnerHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);

        // Act
        var actualResponse = await _testableHandler.SendAsync(request, CancellationToken.None);

        // Assert
        Assert.That(actualResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        _mockInnerHandler.Protected().Verify("SendAsync", Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
    }

    [Test]
    public async Task TestSendGraphQlMutationRequest()
    {
        // Arrange
        var graphqlMutation = """
                              {
                                  "query": "mutation CreateUser($input: CreateUserInput!) { createUser(input: $input) { id name } }",
                                  "variables": {"input": {"name": "Jane Doe", "email": "jane@example.com"}},
                                  "operationName": "CreateUser"
                              }
                              """;

        var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/graphql")
        {
            Content = new StringContent(graphqlMutation, Encoding.UTF8, "application/json")
        };

        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""
                                        {
                                            "data": {
                                                "createUser": {
                                                    "id": "456",
                                                    "name": "Jane Doe"
                                                }
                                            }
                                        }
                                        """, Encoding.UTF8, "application/json")
        };

        _mockInnerHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);

        // Act
        var actualResponse = await _testableHandler.SendAsync(request, CancellationToken.None);

        // Assert
        Assert.That(actualResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var responseContent = await actualResponse.Content.ReadAsStringAsync();
        Assert.That(responseContent, Does.Contain("createUser"));
    }

    [Test]
    public void TestHandleGraphQlRequestWithoutMessageListeners()
    {
        // Arrange
        const string request = "Sample GraphQL request content";

        // Act & Assert - Should not throw exception
        Assert.DoesNotThrowAsync(() => _handler.HandleGraphQlRequest(request));
    }

    [Test]
    public void TestHandleGraphQlResponseWithoutMessageListeners()
    {
        // Arrange
        const string response = "Sample GraphQL response content";

        // Act & Assert - Should not throw exception
        Assert.DoesNotThrowAsync(() => _handler.HandleGraphQlResponse(response));
    }

    [Test]
    public void TestMessageListenerFunctionality()
    {
        // Test without message listener
        Assert.That(_handler.HasMessageListeners(), Is.False);

        // Set message listener
        _handler.SetMessageListener(_mockMessageListeners.Object);
        Assert.That(_handler.HasMessageListeners(), Is.True);
    }

    [Test]
    public async Task TestHandleGraphQlRequestWithMessageListeners()
    {
        // Arrange
        _handler.SetMessageListener(_mockMessageListeners.Object);
        var request = "GraphQL Request Content";

        // Act
        await _handler.HandleGraphQlRequest(request);

        // Assert
        _mockMessageListeners.Verify(ml => ml.OnOutboundMessage(
            It.IsAny<RawMessage>(),
            It.IsAny<TestContext>()), Times.Once);
    }

    [Test]
    public async Task TestHandleGraphQlResponseWithMessageListeners()
    {
        // Arrange
        _handler.SetMessageListener(_mockMessageListeners.Object);
        var response = "GraphQL Response Content";

        // Act
        await _handler.HandleGraphQlResponse(response);

        // Assert
        _mockMessageListeners.Verify(ml => ml.OnInboundMessage(
            It.IsAny<RawMessage>(),
            It.IsAny<TestContext>()), Times.Once);
    }

    [Test]
    public void TestDetectOperationType()
    {
        // Use reflection to test a private method
        var method = typeof(LoggingGraphQlClientHandler)
            .GetMethod("DetectOperationType", BindingFlags.NonPublic | BindingFlags.Static);

        // Test Query detection
        var queryResult = method?.Invoke(null, ["query GetUser { user { id } }"]);
        Assert.That(queryResult, Is.EqualTo("Query"));

        // Test Mutation detection
        var mutationResult = method?.Invoke(null, ["mutation CreateUser { createUser { id } }"]);
        Assert.That(mutationResult, Is.EqualTo("Mutation"));

        // Test Subscription detection
        var subscriptionResult = method?.Invoke(null, ["subscription OnUser { userUpdated { id } }"]);
        Assert.That(subscriptionResult, Is.EqualTo("Subscription"));

        // Test default Query detection (no operation type specified)
        var defaultResult = method?.Invoke(null, ["{ user { id } }"]);
        Assert.That(defaultResult, Is.EqualTo("Query"));

        // Test Unknown detection
        var unknownResult = method?.Invoke(null, [""]);
        Assert.That(unknownResult, Is.EqualTo("Unknown"));

        // Test null input
        var nullResult = method?.Invoke(null, [null]);
        Assert.That(nullResult, Is.EqualTo("Unknown"));
    }

    [Test]
    public async Task TestFormatGraphQlRequestBodyWithValidJson()
    {
        const string jsonBody = """
                                {
                                    "query": "query GetUser { user { id name } }",
                                    "variables": {"id": "123"},
                                    "operationName": "GetUser"
                                }
                                """;

        var result = await _handler.FormatGraphQlRequestBody(jsonBody);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.Contain("=== GraphQL Request ==="));
        Assert.That(result, Does.Contain("Operation Type: Query"));
        Assert.That(result, Does.Contain("Variables:"));
        Assert.That(result, Does.Contain("Operation Name: GetUser"));
        Console.WriteLine(result);
    }

    [Test]
    public async Task TestFormatGraphQlResponseBodyWithValidJson()
    {
        const string jsonBody = """
                                {
                                    "data": {
                                        "user": {
                                            "id": "123",
                                            "name": "John Doe"
                                        }
                                    }
                                }
                                """;


        var result = await _handler.FormatGraphQlResponseBody(jsonBody);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.Contain("=== GraphQL Response ==="));
        Assert.That(result, Does.Contain("Data:"));
    }

    [Test]
    public async Task TestFormatGraphQlResponseBodyWithErrors()
    {
        const string jsonBody = """
                                {
                                    "data": null,
                                    "errors": [
                                        {
                                            "message": "User not found",
                                            "locations": [{"line": 1, "column": 20}],
                                            "path": ["user"]
                                        }
                                    ]
                                }
                                """;

        var result = await _handler.FormatGraphQlResponseBody(jsonBody);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.Contain("=== GraphQL Response ==="));
        Assert.That(result, Does.Contain("Errors:"));
        Assert.That(result, Does.Contain("User not found"));
    }

    [Test]
    public async Task TestFormatGraphQlResponseBodyWithExtensions()
    {
        const string jsonBody = """
                                {
                                    "data": {"user": {"id": "123"}},
                                    "extensions": {
                                        "tracing": {
                                            "version": 1,
                                            "startTime": "2023-01-01T10:00:00Z",
                                            "endTime": "2023-01-01T10:00:01Z"
                                        }
                                    }
                                }
                                """;

        var result = await _handler.FormatGraphQlResponseBody(jsonBody);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.Contain("=== GraphQL Response ==="));
        Assert.That(result, Does.Contain("Data:"));
        Assert.That(result, Does.Contain("Extensions:"));
        Assert.That(result, Does.Contain("tracing"));
    }

    [Test]
    public async Task TestFormatGraphQlRequestBodyWithInvalidJson()
    {
        const string invalidJson = "{ invalid json }";

        var result = await _handler.FormatGraphQlResponseBody(invalidJson);

        // Should return original body when JSON parsing fails
        Assert.That(result, Is.EqualTo(invalidJson));
    }

    [Test]
    public async Task TestFormatGraphQlResponseBodyWithInvalidJson()
    {
        const string invalidJson = "{ invalid json }";

        var result = await _handler.FormatGraphQlResponseBody(invalidJson);

        // Should return the original body when JSON parsing fails
        Assert.That(result, Is.EqualTo(invalidJson));
    }

    [Test]
    public async Task TestFormatGraphQlQuery()
    {
        const string query = "query GetUser { user { id, name, email } }";

        var result = await _handler.FormatGraphQlRequestBody(query);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.Contain("query GetUser { user { id, name, email } }"));
    }

    [Test]
    public async Task TestFormatGraphQlQueryWithEmptyString()
    {
        var result = await _handler.FormatGraphQlRequestBody("");

        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public async Task TestGetGraphQlResponseContentWithNullResponse()
    {
        var result = await _handler.GetGraphQlResponseContent(null);

        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public async Task TestGetGraphQlResponseContentWithValidResponse()
    {
        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"data": {"user": {"id": "123"}}}""", Encoding.UTF8, "application/json")
        };

        var result = await _handler.GetGraphQlResponseContent(response);

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.Contain("HTTP/"));
        Assert.That(result, Does.Contain("200"));
        Assert.That(result, Does.Contain("=== GraphQL Response ==="));
    }

    [Test]
    public void TestIntegrationWithHttpClient()
    {
        // This test demonstrates how the handler would be used in practice
        var client = new HttpClient(_testableHandler);

        const string graphqlQuery = """
                                    {
                                        "query": "query GetUser { user { id name } }",
                                        "variables": null,
                                        "operationName": "GetUser"
                                    }
                                    """;

        var request = new HttpRequestMessage(HttpMethod.Post, "http://localhost/graphql")
        {
            Content = new StringContent(graphqlQuery, Encoding.UTF8, "application/json")
        };

        var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"data": {"user": {"id": "123", "name": "John"}}}""",
                Encoding.UTF8, "application/json")
        };

        _mockInnerHandler.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync", ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(expectedResponse);

        // Act
        var response = client.SendAsync(request).GetAwaiter().GetResult();

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        client.Dispose();
    }
}
