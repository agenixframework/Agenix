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

using System.Net.Mime;
using Agenix.Api.Endpoint.Resolver;
using Agenix.Api.Message;
using Agenix.Api.Messaging;
using Agenix.Core;
using Agenix.Core.Actions;
using Agenix.Core.Message;
using Agenix.GraphQL.Actions;
using Agenix.GraphQL.Client;
using Agenix.GraphQL.Message;
using Moq;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.GraphQL.Tests.Actions.Dsl;

/// <summary>
///     Unit tests for GraphQL message sending test action builder functionality.
///     Tests the construction and execution of GraphQL send actions with various configurations.
/// </summary>
[TestFixture]
public class SendGraphQLMessageTestActionBuilderTest : AbstractNUnitSetUp
{
    // Change these to be actual mock objects, not Mock<T> wrappers
    private readonly GraphQLClient _graphQLClient = Mock.Of<GraphQLClient>();
    private readonly IProducer _messageProducer = Mock.Of<IProducer>();

    /// <summary>
    ///     Tests the fork mode functionality for GraphQL message sending actions.
    ///     Verifies that both synchronous and asynchronous (fork) GraphQL operations are properly handled.
    /// </summary>
    [Test]
    public void TestForkQuery()
    {
        // Use Mock.Get() pattern like the HTTP test
        Mock.Get(_graphQLClient).Reset();
        Mock.Get(_messageProducer).Reset();

        Mock.Get(_graphQLClient).Setup(c => c.CreateProducer()).Returns(_messageProducer);
        var resetEvent = new ManualResetEventSlim(false);
        // Set up the mock to signal when Send is called
        Mock.Get(_messageProducer)
            .Setup(m => m.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((msg, ctx) => resetEvent.Set());


        var builder = new DefaultTestCaseRunner(Context);

        // Act - Send a GraphQL query without fork mode
        builder.Run(GraphQLActionBuilder.GraphQL().Client(_graphQLClient)
            .Send()
            .Query()
            .Message(new DefaultMessage("""
                                                        query GetUser($id: ID!) {
                                                            user(id: $id) {
                                                                id
                                                                name
                                                                email
                                                            }
                                                        }
                                        """).SetHeader("operationName", "GetUser"))
            .Type(MessageType.JSON)
            .Header("variables", @"{""id"": ""123""}"));

        // Wait for the forked operation to complete
        Assert.That(resetEvent.Wait(TimeSpan.FromSeconds(5)), Is.True,
            "The Send method was not called within the expected timeout");

        // Now use Mock.Get() for verification like the HTTP test
        Mock.Get(_messageProducer).Verify(m => m.Send(
            It.Is<IMessage>(msg => msg.GetPayload<string>().Contains("GetUser")),
            It.IsAny<TestContext>()), Times.Once);

        // Rest of your test remains the same
        var test = builder.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(SendMessageAction)));

        // Assert - Verify first action
        var queryAction = (SendMessageAction)test.GetActions()[0];
        Assert.That(queryAction.Name, Is.EqualTo("graphql:send-request"));
        Assert.That(queryAction.Endpoint, Is.EqualTo(_graphQLClient));

        var queryMessageBuilder = (GraphQLMessageBuilder)queryAction.MessageBuilder;
        Assert.That(queryMessageBuilder.GetMessage().GetPayload<string>(), Does.Contain("GetUser"));

        var queryHeaders = queryMessageBuilder.BuildMessageHeaders(Context);
        Assert.That(queryHeaders.Count, Is.EqualTo(4));
        Assert.That(queryHeaders[MessageHeaders.MessageType], Is.EqualTo(nameof(MessageType.PLAINTEXT)));
        Assert.That(queryHeaders[GraphQLMessageHeaders.OperationType], Is.EqualTo(nameof(GraphQLOperationType.QUERY)));
        Assert.That(queryHeaders["operationName"], Is.EqualTo("GetUser"));
        Assert.That(queryHeaders["variables"], Is.EqualTo(@"{""id"": ""123""}"));

        Assert.That(queryAction.ForkMode, Is.False);
    }

    [Test]
    public void TestForkMutation()
    {
        // Use Mock.Get() pattern like the HTTP test
        Mock.Get(_graphQLClient).Reset();
        Mock.Get(_messageProducer).Reset();
        var resetEvent = new ManualResetEventSlim(false);

        Mock.Get(_graphQLClient).Setup(c => c.CreateProducer()).Returns(_messageProducer);

        // Set up the mock to signal when Send is called
        Mock.Get(_messageProducer)
            .Setup(m => m.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((msg, ctx) => resetEvent.Set());


        var builder = new DefaultTestCaseRunner(Context);

        // Act - Send a GraphQL mutation with fork mode enabled
        builder.Run(GraphQLActionBuilder.GraphQL().Client(_graphQLClient)
            .Send()
            .Mutation()
            .Message(new DefaultMessage("""
                                                        mutation CreateUser($input: CreateUserInput!) {
                                                            createUser(input: $input) {
                                                                id
                                                                name
                                                                email
                                                            }
                                                        }
                                        """).SetHeader("operationName", "CreateUser"))
            .Type(MessageType.JSON)
            .Fork(true));

        // Rest of your test remains the same
        var test = builder.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(SendMessageAction)));


        // Assert - Verify second action (Mutation with fork)
        var mutationAction = (SendMessageAction)test.GetActions()[0];
        Assert.That(mutationAction.Name, Is.EqualTo("graphql:send-request"));
        Assert.That(mutationAction.Endpoint, Is.EqualTo(_graphQLClient));

        var mutationMessageBuilder = (GraphQLMessageBuilder)mutationAction.MessageBuilder;
        Assert.That(mutationMessageBuilder.GetMessage().GetPayload<string>(), Does.Contain("CreateUser"));

        var mutationHeaders = mutationMessageBuilder.BuildMessageHeaders(Context);
        Assert.That(mutationHeaders[MessageHeaders.MessageType], Is.EqualTo(nameof(MessageType.PLAINTEXT)));
        Assert.That(mutationHeaders[GraphQLMessageHeaders.OperationType],
            Is.EqualTo(nameof(GraphQLOperationType.MUTATION)));
        Assert.That(mutationHeaders["operationName"], Is.EqualTo("CreateUser"));

        Assert.That(mutationAction.ForkMode, Is.True);

        // Wait for the forked operation to complete
        Assert.That(resetEvent.Wait(TimeSpan.FromSeconds(5)), Is.True,
            "The Send method was not called within the expected timeout");

        Mock.Get(_messageProducer).Verify(m => m.Send(
            It.Is<IMessage>(msg => msg.GetPayload<string>().Contains("CreateUser")),
            It.IsAny<TestContext>()), Times.Once);
    }


    /// <summary>
    ///     Tests GraphQL subscription handling with WebSocket configuration.
    ///     Verifies that subscription operations are properly configured for asynchronous execution.
    /// </summary>
    [Test]
    public void TestSubscription()
    {
        // Arrange
        Mock.Get(_graphQLClient).Reset();
        Mock.Get(_messageProducer).Reset();

        Mock.Get(_graphQLClient).Setup(c => c.CreateProducer()).Returns(_messageProducer);

        var builder = new DefaultTestCaseRunner(Context);

        // Act - Send a GraphQL subscription
        builder.Run(GraphQLActionBuilder.GraphQL().Client(_graphQLClient)
            .Send()
            .Subscription()
            .Message(new DefaultMessage("""
                                                        subscription OnCommentAdded($postId: ID!) {
                                                            commentAdded(postId: $postId) {
                                                                id
                                                                content
                                                                author {
                                                                    name
                                                                }
                                                            }
                                                        }
                                        """).SetHeader("operationName", "OnCommentAdded"))
            .Type(MessageType.JSON)
            .Header("variables", @"{""postId"": ""post-123""}")
            .Header("useWebSocket", "true"));

        // Assert
        Mock.Get(_messageProducer).Verify(m => m.Send(
            It.Is<IMessage>(msg => msg.GetPayload<string>().Contains("OnCommentAdded")),
            It.IsAny<TestContext>()), Times.Once);

        var test = builder.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));

        var subscriptionAction = (SendMessageAction)test.GetActions()[0];
        var subscriptionMessageBuilder = (GraphQLMessageBuilder)subscriptionAction.MessageBuilder;

        var subscriptionHeaders = subscriptionMessageBuilder.BuildMessageHeaders(Context);
        Assert.That(subscriptionHeaders[GraphQLMessageHeaders.OperationType],
            Is.EqualTo(nameof(GraphQLOperationType.SUBSCRIPTION)));
        Assert.That(subscriptionHeaders["useWebSocket"], Is.EqualTo("true"));
        Assert.That(subscriptionHeaders["operationName"], Is.EqualTo("OnCommentAdded"));
    }

    /// <summary>
    ///     Tests GraphQL message builder with authentication headers.
    ///     Verifies that authentication information is properly included in GraphQL requests.
    /// </summary>
    [Test]
    public void TestWithAuthentication()
    {
        // Arrange
        Mock.Get(_graphQLClient).Reset();
        Mock.Get(_messageProducer).Reset();

        Mock.Get(_graphQLClient).Setup(c => c.CreateProducer()).Returns(_messageProducer);

        var builder = new DefaultTestCaseRunner(Context);

        // Act - Send GraphQL query with authentication
        builder.Run(GraphQLActionBuilder.GraphQL().Client(_graphQLClient)
            .Send()
            .Query()
            .Message(new DefaultMessage("""
                                                        query GetPrivateData {
                                                            privateData {
                                                                secret
                                                            }
                                                        }
                                        """))
            .Type(MessageType.JSON)
            .Header("Authorization", "Bearer token-123")
            .Header("X-API-Key", "api-key-456"));

        // Assert
        var test = builder.GetTestCase();
        var action = (SendMessageAction)test.GetActions()[0];
        var messageBuilder = (GraphQLMessageBuilder)action.MessageBuilder;

        var headers = messageBuilder.BuildMessageHeaders(Context);
        Assert.That(headers["Authorization"], Is.EqualTo("Bearer token-123"));
        Assert.That(headers["X-API-Key"], Is.EqualTo("api-key-456"));
        Assert.That(headers[GraphQLMessageHeaders.OperationType],
            Is.EqualTo(nameof(GraphQLOperationType.QUERY)));
    }

    /// <summary>
    ///     Tests GraphQL message builder with custom correlation and retry configuration.
    ///     Verifies that advanced GraphQL features like correlation and retry policies work correctly.
    /// </summary>
    [Test]
    public void TestWithCorrelationAndRetry()
    {
        // Arrange
        Mock.Get(_graphQLClient).Reset();
        Mock.Get(_messageProducer).Reset();

        Mock.Get(_graphQLClient).Setup(c => c.CreateProducer()).Returns(_messageProducer);

        var builder = new DefaultTestCaseRunner(Context);

        // Act - Send GraphQL query with correlation and retry settings
        builder.Run(GraphQLActionBuilder.GraphQL().Client(_graphQLClient)
            .Send()
            .Query()
            .Message(new DefaultMessage("""
                                                        query GetData {
                                                            data {
                                                                value
                                                            }
                                                        }
                                        """))
            .Type(MessageType.JSON)
            .Header("correlationId", "test-correlation-123")
            .Header("maxRetries", "3")
            .Header("retryDelay", "1000")
            .Header("timeout", "30000"));

        // Assert
        var test = builder.GetTestCase();
        var action = (SendMessageAction)test.GetActions()[0];
        var messageBuilder = (GraphQLMessageBuilder)action.MessageBuilder;

        var headers = messageBuilder.BuildMessageHeaders(Context);
        Assert.That(headers["correlationId"], Is.EqualTo("test-correlation-123"));
        Assert.That(headers["maxRetries"], Is.EqualTo("3"));
        Assert.That(headers["retryDelay"], Is.EqualTo("1000"));
        Assert.That(headers["timeout"], Is.EqualTo("30000"));
    }

    /// <summary>
    ///     Tests GraphQL message builder with complex nested operations and variables.
    ///     Verifies that complex GraphQL operations with nested data structures are handled correctly.
    /// </summary>
    [Test]
    public void TestComplexGraphQLOperation()
    {
        // Arrange
        Mock.Get(_graphQLClient).Reset();
        Mock.Get(_messageProducer).Reset();

        Mock.Get(_graphQLClient).Setup(c => c.CreateProducer()).Returns(_messageProducer);

        var builder = new DefaultTestCaseRunner(Context);

        // Act - Send complex GraphQL query with nested selections
        builder.Run(GraphQLActionBuilder.GraphQL().Client(_graphQLClient)
            .Send()
            .Query()
            .Message(new DefaultMessage("""
                                                        query GetUserWithPosts($userId: ID!, $limit: Int!) {
                                                            user(id: $userId) {
                                                                id
                                                                name
                                                                posts(limit: $limit) {
                                                                    edges {
                                                                        node {
                                                                            id
                                                                            title
                                                                            comments {
                                                                                id
                                                                                content
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                        """).SetHeader("operationName", "GetUserWithPosts"))
            .Type(MessageType.JSON)
            .Header("variables", @"{""userId"": ""user-456"", ""limit"": 10}")
            .Header("timeout", "15000"));

        // Assert
        Mock.Get(_messageProducer).Verify(m => m.Send(
            It.Is<IMessage>(msg =>
                msg.GetPayload<string>().Contains("GetUserWithPosts") &&
                msg.GetPayload<string>().Contains("posts(limit: $limit)")),
            It.IsAny<TestContext>()), Times.Once);

        var test = builder.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));

        var action = (SendMessageAction)test.GetActions()[0];
        var messageBuilder = (GraphQLMessageBuilder)action.MessageBuilder;

        var headers = messageBuilder.BuildMessageHeaders(Context);
        Assert.That(headers[GraphQLMessageHeaders.OperationType],
            Is.EqualTo(nameof(GraphQLOperationType.QUERY)));
        Assert.That(headers["operationName"], Is.EqualTo("GetUserWithPosts"));
        Assert.That(headers["variables"], Does.Contain("user-456"));
        Assert.That(headers["variables"], Does.Contain("limit"));
        Assert.That(headers["timeout"], Is.EqualTo("15000"));

        var payload = messageBuilder.GetMessage().GetPayload<string>();
        Assert.That(payload, Does.Contain("GetUserWithPosts"));
        Assert.That(payload, Does.Contain("posts(limit: $limit)"));
        Assert.That(payload, Does.Contain("comments"));
    }

    [Test]
    public void TestMessageObjectOverride()
    {
        Mock.Get(_graphQLClient).Reset();
        Mock.Get(_messageProducer).Reset();

        Mock.Get(_graphQLClient).Setup(c => c.CreateProducer()).Returns(_messageProducer);
        Mock.Get(_messageProducer).Setup(m => m.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, context) =>
            {
                Assert.That(message.GetPayload<string>(), Is.EqualTo("{ user { name email } }"));
            });

        var builder = new DefaultTestCaseRunner(Context);

        builder.Run(GraphQLActionBuilder.GraphQL().Client(_graphQLClient)
            .Send()
            .Query()
            .Message(new GraphQLMessage("{ user { name email } }")
                .SetOperationName("getUserInfo"))
            .Variable("globalVar", "globalValue")
            .Variable("userId", "123")
            .Extension("ext1", "123")
            .Extension("ext2", "456")
            .ContentType(MediaTypeNames.Application.Json)
            .Type(MessageType.PLAINTEXT)
            .Header("additional", "additionalValue"));

        var test = builder.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        var action = (SendMessageAction)test.GetActions()[0];

        var messageBuilder = (GraphQLMessageBuilder)action.MessageBuilder;

        var graphQLMessage = messageBuilder.GetMessage();
        Assert.That(graphQLMessage.GetPayload<string>(), Is.EqualTo("{ user { name email } }"));
        Assert.That(messageBuilder.BuildMessageHeaders(Context).Count, Is.EqualTo(5));
        Assert.That(messageBuilder.BuildMessageHeaders(Context)[GraphQLMessageHeaders.OperationName],
            Is.EqualTo("getUserInfo"));
        Assert.That(messageBuilder.BuildMessageHeaders(Context)["additional"], Is.EqualTo("additionalValue"));
        Assert.That(messageBuilder.BuildMessageHeaders(Context)["Content-Type"],
            Is.EqualTo(MediaTypeNames.Application.Json));
        Assert.That(graphQLMessage.GetVariable("userId"), Is.EqualTo("123"));
        Assert.That(graphQLMessage.GetVariable("globalVar"), Is.EqualTo("globalValue"));
        Assert.That(graphQLMessage.GetExtension("ext1"), Is.EqualTo("123"));
        Assert.That(graphQLMessage.GetExtension("ext2"), Is.EqualTo("456"));
    }

    [Test]
    public void TestMessageBodyOverride()
    {
        Mock.Get(_graphQLClient).Reset();
        Mock.Get(_messageProducer).Reset();

        Mock.Get(_graphQLClient).Setup(c => c.CreateProducer()).Returns(_messageProducer);

        Mock.Get(_messageProducer).Setup(m => m.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, context) =>
            {
                Assert.That(message.GetPayload<string>(), Is.EqualTo("{ user { name email } }"));
                Assert.That(message.GetHeader(GraphQLMessageHeaders.OperationType), Is.EqualTo("QUERY"));
            });

        var builder = new DefaultTestCaseRunner(Context);

        builder.Run(GraphQLActionBuilder.GraphQL().Client(_graphQLClient)
            .Send()
            .Query()
            .Message()
            .Body("{ user { name email } }"));

        var test = builder.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        var action = (SendMessageAction)test.GetActions()[0];

        Assert.That(((GraphQLMessageBuilder)action.MessageBuilder).GetMessage().GetPayload<string>(),
            Is.EqualTo("{ user { name email } }"));
        Assert.That(((GraphQLMessageBuilder)action.MessageBuilder).BuildMessageHeaders(Context).Count, Is.EqualTo(1));
    }

    [Test]
    public void TestMessageQueryOverride()
    {
        Mock.Get(_graphQLClient).Reset();
        Mock.Get(_messageProducer).Reset();

        Mock.Get(_graphQLClient).Setup(c => c.CreateProducer()).Returns(_messageProducer);

        Mock.Get(_messageProducer).Setup(m => m.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, context) =>
            {
                Assert.That(message.GetPayload<string>(), Is.EqualTo("{ user { name email } }"));
                Assert.That(message.GetHeader(GraphQLMessageHeaders.OperationType), Is.EqualTo("QUERY"));
            });

        var builder = new DefaultTestCaseRunner(Context);

        builder.Run(GraphQLActionBuilder.GraphQL().Client(_graphQLClient)
            .Send()
            .Query("{ user { name email } }"));

        var test = builder.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));
        var action = (SendMessageAction)test.GetActions()[0];

        Assert.That(((GraphQLMessageBuilder)action.MessageBuilder).GetMessage().GetPayload<string>(),
            Is.EqualTo("{ user { name email } }"));
        Assert.That(((GraphQLMessageBuilder)action.MessageBuilder).BuildMessageHeaders(Context).Count, Is.EqualTo(1));
    }

    [Test]
    public void TestGraphQLRequestUriAndPath()
    {
        Mock.Get(_graphQLClient).Reset();
        Mock.Get(_messageProducer).Reset();

        Mock.Get(_graphQLClient).Setup(c => c.CreateProducer()).Returns(_messageProducer);

        Mock.Get(_messageProducer).Setup(m => m.Send(It.IsAny<IMessage>(), It.IsAny<TestContext>()))
            .Callback<IMessage, TestContext>((message, context) =>
            {
                Assert.That(message.GetPayload<string>(),
                    Is.EqualTo("{ user(id: \"123\") { name email } }"));
                Assert.That(message.GetHeader(IEndpointUriResolver.EndpointUriHeaderName),
                    Is.EqualTo("http://localhost:8080/graphql"));
            });

        var builder = new DefaultTestCaseRunner(Context);

        builder.Run(GraphQLActionBuilder.GraphQL()
            .Client(_graphQLClient)
            .Send()
            .Query("{ user(id: \"123\") { name email } }")
            .Uri("http://localhost:8080/graphql"));

        var test = builder.GetTestCase();
        Assert.That(test.GetActionCount(), Is.EqualTo(1));

        Assert.That(test.GetActions()[0].GetType(), Is.EqualTo(typeof(SendMessageAction)));

        var action = (SendMessageAction)test.GetActions()[0];
        Assert.That(action.Name, Is.EqualTo("graphql:send-request"));
        Assert.That(action.Endpoint, Is.EqualTo(_graphQLClient));
        Assert.That(action.MessageBuilder.GetType(), Is.EqualTo(typeof(GraphQLMessageBuilder)));

        var messageBuilder = (GraphQLMessageBuilder)action.MessageBuilder;
        Assert.That(messageBuilder.GetMessage().GetPayload<string>(),
            Is.EqualTo("{ user(id: \"123\") { name email } }"));
        Assert.That(messageBuilder.BuildMessageHeaders(Context).Count, Is.EqualTo(3));
        Assert.That(messageBuilder.BuildMessageHeaders(Context)[GraphQLMessageHeaders.OperationType],
            Is.EqualTo("QUERY"));
        Assert.That(messageBuilder.BuildMessageHeaders(Context)[IEndpointUriResolver.EndpointUriHeaderName],
            Is.EqualTo("http://localhost:8080/graphql"));
    }
}
