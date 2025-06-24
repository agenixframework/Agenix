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
        _handler = new LoggingGraphQLClientHandler(_mockInnerHandler.Object);
        _testableHandler = new TestableLoggingGraphQLClientHandler(_mockInnerHandler.Object);
        _mockMessageListeners = new Mock<MessageListeners>();

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
    private LoggingGraphQLClientHandler _handler;
    private Mock<MessageListeners> _mockMessageListeners;
    private TestableLoggingGraphQLClientHandler _testableHandler;

    // Testable wrapper that exposes the protected Send method
    private class TestableLoggingGraphQLClientHandler : LoggingGraphQLClientHandler
    {
        public TestableLoggingGraphQLClientHandler(HttpMessageHandler innerHandler) : base(innerHandler)
        {
        }

        public new HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return base.Send(request, cancellationToken);
        }
    }

    [Test]
    public void TestSendGraphQLQueryRequest()
    {
        // Arrange
        var graphqlQuery = """
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
            .Setup<HttpResponseMessage>("Send", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var actualResponse = _testableHandler.Send(request, CancellationToken.None);

        // Assert
        Assert.That(actualResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        _mockInnerHandler.Protected().Verify("Send", Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
    }

    [Test]
    public void TestSendRequestWithoutContent()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "http://localhost/graphql");

        var expectedResponse = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Bad Request", Encoding.UTF8, "text/plain")
        };

        _mockInnerHandler.Protected()
            .Setup<HttpResponseMessage>("Send", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var actualResponse = _testableHandler.Send(request, CancellationToken.None);

        // Assert
        Assert.That(actualResponse.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        _mockInnerHandler.Protected().Verify("Send", Times.Once(),
            ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>());
    }

    [Test]
    public void TestSendGraphQLMutationRequest()
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
            .Setup<HttpResponseMessage>("Send", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var actualResponse = _testableHandler.Send(request, CancellationToken.None);

        // Assert
        Assert.That(actualResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        var responseContent = actualResponse.Content.ReadAsStringAsync().Result;
        Assert.That(responseContent, Does.Contain("createUser"));
    }

    [Test]
    public void TestHandleGraphQLRequestWithoutMessageListeners()
    {
        // Arrange
        var request = "Sample GraphQL request content";

        // Act & Assert - Should not throw exception
        Assert.DoesNotThrow(() => _handler.HandleGraphQLRequest(request));
    }

    [Test]
    public void TestHandleGraphQLResponseWithoutMessageListeners()
    {
        // Arrange
        var response = "Sample GraphQL response content";

        // Act & Assert - Should not throw exception
        Assert.DoesNotThrow(() => _handler.HandleGraphQLResponse(response));
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
    public void TestHandleGraphQLRequestWithMessageListeners()
    {
        // Arrange
        _handler.SetMessageListener(_mockMessageListeners.Object);
        var request = "GraphQL Request Content";

        // Act
        _handler.HandleGraphQLRequest(request);

        // Assert
        _mockMessageListeners.Verify(ml => ml.OnOutboundMessage(
            It.IsAny<RawMessage>(),
            It.IsAny<TestContext>()), Times.Once);
    }

    [Test]
    public void TestHandleGraphQLResponseWithMessageListeners()
    {
        // Arrange
        _handler.SetMessageListener(_mockMessageListeners.Object);
        var response = "GraphQL Response Content";

        // Act
        _handler.HandleGraphQLResponse(response);

        // Assert
        _mockMessageListeners.Verify(ml => ml.OnInboundMessage(
            It.IsAny<RawMessage>(),
            It.IsAny<TestContext>()), Times.Once);
    }

    [Test]
    public void TestDetectOperationType()
    {
        // Use reflection to test private method
        var method = typeof(LoggingGraphQLClientHandler)
            .GetMethod("DetectOperationType", BindingFlags.NonPublic | BindingFlags.Instance);

        // Test Query detection
        var queryResult = method?.Invoke(_handler, ["query GetUser { user { id } }"]);
        Assert.That(queryResult, Is.EqualTo("Query"));

        // Test Mutation detection
        var mutationResult = method?.Invoke(_handler, ["mutation CreateUser { createUser { id } }"]);
        Assert.That(mutationResult, Is.EqualTo("Mutation"));

        // Test Subscription detection
        var subscriptionResult = method?.Invoke(_handler, ["subscription OnUser { userUpdated { id } }"]);
        Assert.That(subscriptionResult, Is.EqualTo("Subscription"));

        // Test default Query detection (no operation type specified)
        var defaultResult = method?.Invoke(_handler, ["{ user { id } }"]);
        Assert.That(defaultResult, Is.EqualTo("Query"));

        // Test Unknown detection
        var unknownResult = method?.Invoke(_handler, [""]);
        Assert.That(unknownResult, Is.EqualTo("Unknown"));

        // Test null input
        var nullResult = method?.Invoke(_handler, [null]);
        Assert.That(nullResult, Is.EqualTo("Unknown"));
    }

    [Test]
    public void TestFormatGraphQLRequestBodyWithValidJson()
    {
        // Use reflection to test private method
        var method = typeof(LoggingGraphQLClientHandler)
            .GetMethod("FormatGraphQLRequestBody", BindingFlags.NonPublic | BindingFlags.Instance);

        var jsonBody = """
                       {
                           "query": "query GetUser { user { id name } }",
                           "variables": {"id": "123"},
                           "operationName": "GetUser"
                       }
                       """;

        var result = method?.Invoke(_handler, [jsonBody]) as string;

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.Contain("=== GraphQL Request ==="));
        Assert.That(result, Does.Contain("Operation Type: Query"));
        Assert.That(result, Does.Contain("Variables:"));
        Assert.That(result, Does.Contain("Operation Name: GetUser"));
    }

    [Test]
    public void TestFormatGraphQLResponseBodyWithValidJson()
    {
        // Use reflection to test private method
        var method = typeof(LoggingGraphQLClientHandler)
            .GetMethod("FormatGraphQLResponseBody", BindingFlags.NonPublic | BindingFlags.Instance);

        var jsonBody = """
                       {
                           "data": {
                               "user": {
                                   "id": "123",
                                   "name": "John Doe"
                               }
                           }
                       }
                       """;

        var result = method?.Invoke(_handler, [jsonBody]) as string;

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.Contain("=== GraphQL Response ==="));
        Assert.That(result, Does.Contain("Data:"));
    }

    [Test]
    public void TestFormatGraphQLResponseBodyWithErrors()
    {
        // Use reflection to test private method
        var method = typeof(LoggingGraphQLClientHandler)
            .GetMethod("FormatGraphQLResponseBody", BindingFlags.NonPublic | BindingFlags.Instance);

        var jsonBody = """
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

        var result = method?.Invoke(_handler, [jsonBody]) as string;

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.Contain("=== GraphQL Response ==="));
        Assert.That(result, Does.Contain("Errors:"));
        Assert.That(result, Does.Contain("User not found"));
    }

    [Test]
    public void TestFormatGraphQLResponseBodyWithExtensions()
    {
        // Use reflection to test private method
        var method = typeof(LoggingGraphQLClientHandler)
            .GetMethod("FormatGraphQLResponseBody", BindingFlags.NonPublic | BindingFlags.Instance);

        var jsonBody = """
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

        var result = method?.Invoke(_handler, [jsonBody]) as string;

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.Contain("=== GraphQL Response ==="));
        Assert.That(result, Does.Contain("Data:"));
        Assert.That(result, Does.Contain("Extensions:"));
        Assert.That(result, Does.Contain("tracing"));
    }

    [Test]
    public void TestFormatGraphQLRequestBodyWithInvalidJson()
    {
        // Use reflection to test private method
        var method = typeof(LoggingGraphQLClientHandler)
            .GetMethod("FormatGraphQLRequestBody", BindingFlags.NonPublic | BindingFlags.Instance);

        var invalidJson = "{ invalid json }";

        var result = method?.Invoke(_handler, [invalidJson]) as string;

        // Should return original body when JSON parsing fails
        Assert.That(result, Is.EqualTo(invalidJson));
    }

    [Test]
    public void TestFormatGraphQLResponseBodyWithInvalidJson()
    {
        // Use reflection to test private method
        var method = typeof(LoggingGraphQLClientHandler)
            .GetMethod("FormatGraphQLResponseBody", BindingFlags.NonPublic | BindingFlags.Instance);

        var invalidJson = "{ invalid json }";

        var result = method?.Invoke(_handler, [invalidJson]) as string;

        // Should return original body when JSON parsing fails
        Assert.That(result, Is.EqualTo(invalidJson));
    }

    [Test]
    public void TestFormatGraphQLQuery()
    {
        // Use reflection to test private method
        var method = typeof(LoggingGraphQLClientHandler)
            .GetMethod("FormatGraphQLQuery", BindingFlags.NonPublic | BindingFlags.Instance);

        var query = "query GetUser { user { id, name, email } }";

        var result = method?.Invoke(_handler, [query]) as string;

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Does.Contain("{\n"));
        Assert.That(result, Does.Contain(",\n"));
    }

    [Test]
    public void TestFormatGraphQLQueryWithEmptyString()
    {
        // Use reflection to test private method
        var method = typeof(LoggingGraphQLClientHandler)
            .GetMethod("FormatGraphQLQuery", BindingFlags.NonPublic | BindingFlags.Instance);

        var result = method?.Invoke(_handler, [""]) as string;

        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void TestFormatGraphQLQueryWithNull()
    {
        // Use reflection to test private method
        var method = typeof(LoggingGraphQLClientHandler)
            .GetMethod("FormatGraphQLQuery", BindingFlags.NonPublic | BindingFlags.Instance);

        var result = method?.Invoke(_handler, [null]) as string;

        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void TestFormatGraphQLRequestBodyWithEmptyString()
    {
        // Use reflection to test private method
        var method = typeof(LoggingGraphQLClientHandler)
            .GetMethod("FormatGraphQLRequestBody", BindingFlags.NonPublic | BindingFlags.Instance);

        var result = method?.Invoke(_handler, [""]) as string;

        Assert.That(result, Is.EqualTo(""));
    }

    [Test]
    public void TestFormatGraphQLResponseBodyWithEmptyString()
    {
        // Use reflection to test private method
        var method = typeof(LoggingGraphQLClientHandler)
            .GetMethod("FormatGraphQLResponseBody", BindingFlags.NonPublic | BindingFlags.Instance);

        var result = method?.Invoke(_handler, [""]) as string;

        Assert.That(result, Is.EqualTo(""));
    }

    [Test]
    public void TestGetGraphQLResponseContentWithNullResponse()
    {
        // Use reflection to test private method
        var method = typeof(LoggingGraphQLClientHandler)
            .GetMethod("GetGraphQLResponseContent", BindingFlags.NonPublic | BindingFlags.Instance);

        var result = method?.Invoke(_handler, [null]) as string;

        Assert.That(result, Is.EqualTo(string.Empty));
    }

    [Test]
    public void TestGetGraphQLResponseContentWithValidResponse()
    {
        // Use reflection to test private method
        var method = typeof(LoggingGraphQLClientHandler)
            .GetMethod("GetGraphQLResponseContent", BindingFlags.NonPublic | BindingFlags.Instance);

        var response = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("""{"data": {"user": {"id": "123"}}}""", Encoding.UTF8, "application/json")
        };

        var result = method?.Invoke(_handler, [response]) as string;

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

        var graphqlQuery = """
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
            .Setup<HttpResponseMessage>("Send", ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .Returns(expectedResponse);

        // Act
        var response = client.Send(request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        client.Dispose();
    }
}
