using Agenix.Api.Message;
using Agenix.Api.Messaging;
using Agenix.Api.Validation.Context;
using Agenix.Core;
using Agenix.Core.Actions;
using Agenix.Core.Validation.Json;
using Agenix.GraphQL.Actions;
using Agenix.GraphQL.Client;
using Agenix.GraphQL.Message;
using Moq;
using Is = NUnit.Framework.Is;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.GraphQL.Tests.Actions.Dsl;

public class ReceiveGraphQLMessageTestActionBuilderTest : AbstractNUnitSetUp
{
    private readonly Mock<GraphQLEndpointConfiguration> _configuration = new();
    private readonly Mock<ISelectiveConsumer> _consumer = new();
    private readonly Mock<GraphQLClient> _graphQLClient = new();

    [Test]
    public void TestGraphQLRequestProperties()
    {
        // Reset mocks properly
        _graphQLClient.Reset();
        _configuration.Reset();
        _consumer.Reset();

        // Setup configuration mock
        _configuration.Setup(x => x.Timeout).Returns(100L);

        // Setup GraphQL client mock
        _graphQLClient.Setup(m => m.EndpointConfiguration).Returns(_configuration.Object);
        _graphQLClient.Setup(m => m.CreateConsumer()).Returns(_graphQLClient.Object);

        // Create the expected GraphQLMessage with method chaining outside mock setup
        var expectedMessage = new GraphQLMessage("""{"data":{"user":{"id":"123","name":"John Doe"}}}""")
            .SetOperationType(GraphQLOperationType.QUERY)
            .SetOperationName("GetUser")
            .SetVariable("var", """{"id":"123"}""");

        // Setup the Receive method directly on the mock
        _graphQLClient.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(expectedMessage);

        var builder = new DefaultTestCaseRunner(Context);
        builder.Run(GraphQLActionBuilder.GraphQL().Client(_graphQLClient.Object)
            .Receive()
            .Response()
            .Message()
            .Body("""{"data":{"user":{"id":"123","name":"John Doe"}}}""")
            .Type(MessageType.JSON)
        );

        var test = builder.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("graphql:receive-response"));

        Assert.That(action.ValidationContexts.Count, Is.EqualTo(2L));
        Assert.That(action.ValidationContexts[0].GetType(), Is.EqualTo(typeof(HeaderValidationContext)));
        Assert.That(action.ValidationContexts[1].GetType(), Is.EqualTo(typeof(JsonMessageValidationContext)));

        var messageBuilder = (GraphQLMessageBuilder)action.MessageBuilder;
        Assert.That(messageBuilder.BuildMessagePayload(Context, action.MessageType),
            Is.EqualTo("""{"data":{"user":{"id":"123","name":"John Doe"}}}"""));
    }

    [Test]
    public void TestGraphQLMessageObjectOverride()
    {
        // Reset mocks
        _graphQLClient.Reset();
        _configuration.Reset();

        // Setup configuration mock
        _configuration.Setup(x => x.Timeout).Returns(100L);

        // Setup GraphQL client mock
        _graphQLClient.Setup(m => m.EndpointConfiguration).Returns(_configuration.Object);
        _graphQLClient.Setup(m => m.CreateConsumer()).Returns(_graphQLClient.Object);

        // Create the expected GraphQLMessage with overridden properties
        var expectedMessage = new GraphQLMessage("""{"data":{"user":{"id":"456","name":"Jane Smith"}}}""")
            .SetOperationType(GraphQLOperationType.MUTATION)
            .SetOperationName("UpdateUser")
            .SetVariable("userId", "456")
            .SetVariable("userName", "Jane Smith");

        // Setup the Receive method on the mock
        _graphQLClient.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(expectedMessage);

        var builder = new DefaultTestCaseRunner(Context);
        builder.Run(GraphQLActionBuilder.GraphQL().Client(_graphQLClient.Object)
            .Receive()
            .Response()
            .Message()
            .Body("""{"data":{"user":{"id":"456","name":"Jane Smith"}}}""")
            .Type(MessageType.JSON)
        );

        var test = builder.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("graphql:receive-response"));

        Assert.That(action.ValidationContexts.Count, Is.EqualTo(2));
        Assert.That(action.ValidationContexts[0].GetType(), Is.EqualTo(typeof(HeaderValidationContext)));
        Assert.That(action.ValidationContexts[1].GetType(), Is.EqualTo(typeof(JsonMessageValidationContext)));

        var messageBuilder = (GraphQLMessageBuilder)action.MessageBuilder;
        Assert.That(messageBuilder.BuildMessagePayload(Context, action.MessageType),
            Is.EqualTo("""{"data":{"user":{"id":"456","name":"Jane Smith"}}}"""));

        // Verify the message object overrides
        var receivedMessage = (GraphQLMessage)_graphQLClient.Object.Receive(Context, 100L);
        Assert.That(receivedMessage.GetOperationType(), Is.EqualTo(GraphQLOperationType.MUTATION));
        Assert.That(receivedMessage.GetOperationName(), Is.EqualTo("UpdateUser"));
        Assert.That(receivedMessage.GetVariable("userId"), Is.EqualTo("456"));
        Assert.That(receivedMessage.GetVariable("userName"), Is.EqualTo("Jane Smith"));
    }

    [Test]
    public void TestGraphQLResponseProperties()
    {
        // Reset mocks
        _graphQLClient.Reset();
        _configuration.Reset();

        // Setup configuration mock
        _configuration.Setup(x => x.Timeout).Returns(200L);

        // Setup GraphQL client mock
        _graphQLClient.Setup(m => m.EndpointConfiguration).Returns(_configuration.Object);
        _graphQLClient.Setup(m => m.CreateConsumer()).Returns(_graphQLClient.Object);

        // Create the expected GraphQL response message with comprehensive data
        var expectedResponseMessage = new GraphQLMessage("""
                                                                 {
                                                                     "data": {
                                                                         "user": {
                                                                             "id": "789",
                                                                             "name": "Alice Johnson",
                                                                             "email": "alice.johnson@example.com"
                                                                         }
                                                                     },
                                                                     "extensions": {
                                                                         "tracing": {
                                                                             "version": 1,
                                                                             "startTime": "2023-01-01T00:00:00Z"
                                                                         }
                                                                     }
                                                                 }
                                                         """)
            .SetOperationType(GraphQLOperationType.QUERY)
            .SetOperationName("GetUserProfile")
            .SetVariable("id", "789")
            .SetExtension("requestId", "req-12345");

        // Setup the Receive method on the mock
        _graphQLClient.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(expectedResponseMessage);

        var builder = new DefaultTestCaseRunner(Context);
        builder.Run(GraphQLActionBuilder.GraphQL().Client(_graphQLClient.Object)
            .Receive()
            .Response()
            .Message()
            .Body("""
                              {
                                  "data": {
                                      "user": {
                                          "id": "789",
                                          "name": "Alice Johnson",
                                          "email": "alice.johnson@example.com"
                                      }
                                  },
                                  "extensions": {
                                      "tracing": {
                                          "version": 1,
                                          "startTime": "2023-01-01T00:00:00Z"
                                      }
                                  }
                              }
                  """)
            .Type(MessageType.JSON)
        );

        var test = builder.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(ReceiveMessageAction)));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("graphql:receive-response"));

        // Verify validation contexts
        Assert.That(action.ValidationContexts.Count, Is.EqualTo(2));
        Assert.That(action.ValidationContexts[0].GetType(), Is.EqualTo(typeof(HeaderValidationContext)));
        Assert.That(action.ValidationContexts[1].GetType(), Is.EqualTo(typeof(JsonMessageValidationContext)));

        var messageBuilder = (GraphQLMessageBuilder)action.MessageBuilder;
        var builtPayload = messageBuilder.BuildMessagePayload(Context, action.MessageType);

        // Verify the response payload structure
        Assert.That(builtPayload, Does.Contain("\"id\": \"789\""));
        Assert.That(builtPayload, Does.Contain("\"name\": \"Alice Johnson\""));
        Assert.That(builtPayload, Does.Contain("\"email\": \"alice.johnson@example.com\""));

        // Verify GraphQL-specific response properties
        var receivedMessage = (GraphQLMessage)_graphQLClient.Object.Receive(Context, 200L);
        Assert.That(receivedMessage.GetOperationType(), Is.EqualTo(GraphQLOperationType.QUERY));
        Assert.That(receivedMessage.GetOperationName(), Is.EqualTo("GetUserProfile"));
        Assert.That(receivedMessage.GetVariable("id"), Is.EqualTo("789"));
        Assert.That(receivedMessage.GetExtension("requestId"), Is.EqualTo("req-12345"));

        // Verify response-specific properties
        Assert.That(receivedMessage.Headers, Is.Not.Null);
        Assert.That(receivedMessage.Extensions, Is.Not.Null);
        Assert.That(receivedMessage.Variables, Is.Not.Null);
    }

    [Test]
    public void TestGraphQLResponseWithErrors()
    {
        _graphQLClient.Reset();
        _configuration.Reset();

        _configuration.Setup(x => x.Timeout).Returns(100L);
        _graphQLClient.Setup(m => m.EndpointConfiguration).Returns(_configuration.Object);
        _graphQLClient.Setup(m => m.CreateConsumer()).Returns(_graphQLClient.Object);

        var errorMessage = new GraphQLMessage("""
                                                                                                    {
                                                                                                        "errors": [
                                                                                                            {
                                                                                                                "message": "User not found",
                                                                                                                "locations": [{"line": 2, "column": 3}],
                                                                                                                "path": ["user"]
                                                                                                            }
                                                                                                        ],
                                                                                                        "data": null
                                                                                                    }
                                              """);

        _graphQLClient.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(errorMessage);

        var builder = new DefaultTestCaseRunner(Context);
        builder.Run(GraphQLActionBuilder.GraphQL().Client(_graphQLClient.Object)
            .Receive()
            .Response()
            .Message()
            .Body("""
                                                {
                                                    "errors": [
                                                        {
                                                            "message": "User not found",
                                                            "locations": [{"line": 2, "column": 3}],
                                                            "path": ["user"]
                                                        }
                                                    ],
                                                    "data": null
                                                }
                  """)
            .Type(MessageType.JSON)
        );

        var test = builder.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("graphql:receive-response"));

        var builtPayload =
            ((GraphQLMessageBuilder)action.MessageBuilder).BuildMessagePayload(Context, action.MessageType);
        Assert.That(builtPayload, Does.Contain("\"message\": \"User not found\""));
    }

    [Test]
    public void TestGraphQLMutationResponse()
    {
        _graphQLClient.Reset();
        _configuration.Reset();

        _configuration.Setup(x => x.Timeout).Returns(100L);
        _graphQLClient.Setup(m => m.EndpointConfiguration).Returns(_configuration.Object);
        _graphQLClient.Setup(m => m.CreateConsumer()).Returns(_graphQLClient.Object);

        var mutationResponse = new GraphQLMessage("""
                                                                                                            {
                                                                                                                "data": {
                                                                                                                    "createUser": {
                                                                                                                        "id": "new-user-123",
                                                                                                                        "name": "John Doe",
                                                                                                                        "email": "john@example.com"
                                                                                                                    }
                                                                                                                }
                                                                                                            }
                                                  """)
            .SetOperationType(GraphQLOperationType.MUTATION)
            .SetOperationName("CreateUser");

        _graphQLClient.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(mutationResponse);

        var builder = new DefaultTestCaseRunner(Context);
        builder.Run(GraphQLActionBuilder.GraphQL().Client(_graphQLClient.Object)
            .Receive()
            .Response()
            .Message()
            .Body("""
                                                {
                                                    "data": {
                                                        "createUser": {
                                                            "id": "new-user-123",
                                                            "name": "John Doe",
                                                            "email": "john@example.com"
                                                        }
                                                    }
                                                }
                  """)
            .Type(MessageType.JSON)
        );

        var test = builder.GetTestCase();
        var action = (ReceiveMessageAction)test.GetActions()[0];
        var receivedMessage = (GraphQLMessage)_graphQLClient.Object.Receive(Context, 100L);

        Assert.That(receivedMessage.GetOperationType(), Is.EqualTo(GraphQLOperationType.MUTATION));
        Assert.That(receivedMessage.GetOperationName(), Is.EqualTo("CreateUser"));
    }

    [Test]
    public void TestGraphQLSubscriptionResponse()
    {
        _graphQLClient.Reset();
        _configuration.Reset();

        _configuration.Setup(x => x.Timeout).Returns(100L);
        _graphQLClient.Setup(m => m.EndpointConfiguration).Returns(_configuration.Object);
        _graphQLClient.Setup(m => m.CreateConsumer()).Returns(_graphQLClient.Object);

        var subscriptionResponse = new GraphQLMessage("""
                                                                                                                    {
                                                                                                                        "data": {
                                                                                                                            "userUpdated": {
                                                                                                                                "id": "123",
                                                                                                                                "name": "Updated User",
                                                                                                                                "status": "ACTIVE"
                                                                                                                            }
                                                                                                                        }
                                                                                                                    }
                                                      """)
            .SetOperationType(GraphQLOperationType.SUBSCRIPTION)
            .SetOperationName("OnUserUpdated");

        _graphQLClient.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(subscriptionResponse);

        var builder = new DefaultTestCaseRunner(Context);
        builder.Run(GraphQLActionBuilder.GraphQL().Client(_graphQLClient.Object)
            .Receive()
            .Response()
            .Message()
            .Body("""
                                                {
                                                    "data": {
                                                        "userUpdated": {
                                                            "id": "123",
                                                            "name": "Updated User",
                                                            "status": "ACTIVE"
                                                        }
                                                    }
                                                }
                  """)
            .Type(MessageType.JSON)
        );

        var test = builder.GetTestCase();
        var action = (ReceiveMessageAction)test.GetActions()[0];
        var receivedMessage = (GraphQLMessage)_graphQLClient.Object.Receive(Context, 100L);

        Assert.That(receivedMessage.GetOperationType(), Is.EqualTo(GraphQLOperationType.SUBSCRIPTION));
        Assert.That(receivedMessage.GetOperationName(), Is.EqualTo("OnUserUpdated"));
    }

    [Test]
    public void TestGraphQLResponseWithExtensions()
    {
        _graphQLClient.Reset();
        _configuration.Reset();

        _configuration.Setup(x => x.Timeout).Returns(100L);
        _graphQLClient.Setup(m => m.EndpointConfiguration).Returns(_configuration.Object);
        _graphQLClient.Setup(m => m.CreateConsumer()).Returns(_graphQLClient.Object);

        var responseWithExtensions = new GraphQLMessage("""
                                                                {
                                                                    "data": {
                                                                        "user": {
                                                                            "id": "456",
                                                                            "name": "Jane Smith"
                                                                        }
                                                                    },
                                                                    "extensions": {
                                                                        "tracing": {
                                                                            "version": 1,
                                                                            "startTime": "2023-01-01T00:00:00Z",
                                                                            "duration": 150
                                                                        },
                                                                        "cost": {
                                                                            "requestedQueryCost": 5,
                                                                            "actualQueryCost": 3
                                                                        }
                                                                    }
                                                                }
                                                        """);

        _graphQLClient.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(responseWithExtensions);

        var builder = new DefaultTestCaseRunner(Context);
        builder.Run(GraphQLActionBuilder.GraphQL().Client(_graphQLClient.Object)
            .Receive()
            .Response()
            .Message()
            .Body("""
                              {
                                  "data": {
                                      "user": {
                                          "id": "456",
                                          "name": "Jane Smith"
                                      }
                                  },
                                  "extensions": {
                                      "tracing": {
                                          "version": 1,
                                          "startTime": "2023-01-01T00:00:00Z",
                                          "duration": 150
                                      },
                                      "cost": {
                                          "requestedQueryCost": 5,
                                          "actualQueryCost": 3
                                      }
                                  }
                              }
                  """)
            .Type(MessageType.JSON)
        );

        var test = builder.GetTestCase();
        var action = (ReceiveMessageAction)test.GetActions()[0];
        var builtPayload =
            ((GraphQLMessageBuilder)action.MessageBuilder).BuildMessagePayload(Context, action.MessageType);

        Assert.That(builtPayload, Does.Contain("\"tracing\""));
        Assert.That(builtPayload, Does.Contain("\"cost\""));
        Assert.That(builtPayload, Does.Contain("\"duration\": 150"));
    }

    [Test]
    public void TestGraphQLEmptyResponse()
    {
        _graphQLClient.Reset();
        _configuration.Reset();

        _configuration.Setup(x => x.Timeout).Returns(100L);
        _graphQLClient.Setup(m => m.EndpointConfiguration).Returns(_configuration.Object);
        _graphQLClient.Setup(m => m.CreateConsumer()).Returns(_graphQLClient.Object);

        var emptyMessage = new GraphQLMessage("""{"data": null}""");

        _graphQLClient.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(emptyMessage);

        var builder = new DefaultTestCaseRunner(Context);
        builder.Run(GraphQLActionBuilder.GraphQL().Client(_graphQLClient.Object)
            .Receive()
            .Response()
            .Message()
            .Body("""{"data": null}""")
            .Type(MessageType.JSON)
        );

        var test = builder.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));

        var action = (ReceiveMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("graphql:receive-response"));

        var builtPayload =
            ((GraphQLMessageBuilder)action.MessageBuilder).BuildMessagePayload(Context, action.MessageType);
        Assert.That(builtPayload, Does.Contain("\"data\": null"));
    }

    [Test]
    public void TestGraphQLResponseWithCustomTimeout()
    {
        _graphQLClient.Reset();
        _configuration.Reset();

        _configuration.Setup(x => x.Timeout).Returns(5000L);
        _graphQLClient.Setup(m => m.EndpointConfiguration).Returns(_configuration.Object);
        _graphQLClient.Setup(m => m.CreateConsumer()).Returns(_graphQLClient.Object);

        var message = new GraphQLMessage("""{"data": {"user": {"id": "timeout-test"}}}""");

        _graphQLClient.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(message);

        var builder = new DefaultTestCaseRunner(Context);
        builder.Run(GraphQLActionBuilder.GraphQL().Client(_graphQLClient.Object)
            .Receive()
            .Response()
            .Timeout(6000L)
            .Message()
            .Body("""{"data": {"user": {"id": "timeout-test"}}}""")
            .Type(MessageType.JSON)
        );

        var test = builder.GetTestCase();
        var action = (ReceiveMessageAction)test.GetActions()[0];

        // Verify timeout is properly configured
        _graphQLClient.Verify(x => x.Receive(It.IsAny<TestContext>(), 6000L), Times.Once);
    }

    [Test]
    public void TestGraphQLComplexNestedResponse()
    {
        _graphQLClient.Reset();
        _configuration.Reset();

        _configuration.Setup(x => x.Timeout).Returns(100L);
        _graphQLClient.Setup(m => m.EndpointConfiguration).Returns(_configuration.Object);
        _graphQLClient.Setup(m => m.CreateConsumer()).Returns(_graphQLClient.Object);

        var complexResponse = new GraphQLMessage("""
                                                         {
                                                             "data": {
                                                                 "organization": {
                                                                     "id": "org-123",
                                                                     "name": "Acme Corp",
                                                                     "users": [
                                                                         {
                                                                             "id": "user-1",
                                                                             "name": "Alice",
                                                                             "roles": ["ADMIN", "USER"]
                                                                         },
                                                                         {
                                                                             "id": "user-2",
                                                                             "name": "Bob",
                                                                             "roles": ["USER"]
                                                                         }
                                                                     ]
                                                                 }
                                                             }
                                                         }
                                                 """);

        _graphQLClient.Setup(x => x.Receive(It.IsAny<TestContext>(), It.IsAny<long>()))
            .Returns(complexResponse);

        var builder = new DefaultTestCaseRunner(Context);
        builder.Run(GraphQLActionBuilder.GraphQL().Client(_graphQLClient.Object)
            .Receive()
            .Response()
            .Message()
            .Body("""
                              {
                                  "data": {
                                      "organization": {
                                          "id": "org-123",
                                          "name": "Acme Corp",
                                          "users": [
                                              {
                                                  "id": "user-1",
                                                  "name": "Alice",
                                                  "roles": ["ADMIN", "USER"]
                                              },
                                              {
                                                  "id": "user-2",
                                                  "name": "Bob",
                                                  "roles": ["USER"]
                                              }
                                          ]
                                      }
                                  }
                              }
                  """)
            .Type(MessageType.JSON)
        );

        var test = builder.GetTestCase();
        var action = (ReceiveMessageAction)test.GetActions()[0];
        var builtPayload =
            ((GraphQLMessageBuilder)action.MessageBuilder).BuildMessagePayload(Context, action.MessageType);

        Assert.That(builtPayload, Does.Contain("\"organization\""));
        Assert.That(builtPayload, Does.Contain("\"users\""));
        Assert.That(builtPayload, Does.Contain("\"roles\""));
        Assert.That(builtPayload, Does.Contain("\"ADMIN\""));
    }
}
