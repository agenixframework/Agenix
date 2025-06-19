#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#endregion

using System.Reactive.Linq;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Message.Correlation;
using Agenix.Api.Messaging;
using Agenix.Core.Endpoint;
using Agenix.Core.Message.Correlation;
using Agenix.GraphQL.Interceptor;
using Agenix.GraphQL.Message;
using GraphQL;
using GraphQL.Client.Http;
using Microsoft.Extensions.Logging;

namespace Agenix.GraphQL.Client;

/// <summary>
///     GraphQL client implementation that extends AbstractEndpoint and provides GraphQL-specific functionality.
///     Supports queries, mutations, and subscriptions with full Agenix integration.
/// </summary>
public class GraphQLClient : AbstractEndpoint, IProducer, IReplyConsumer
{
    /// <summary>
    ///     Logger for this GraphQL client.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(GraphQLClient));

    private readonly Dictionary<string, IObservable<GraphQLResponse<object>>> _activeSubscriptions = new();
    private readonly object _subscriptionLock = new();

    /// <summary>
    ///     Store of reply messages for correlation management.
    /// </summary>
    private ICorrelationManager<IMessage> _correlationManager;

    /// <summary>
    ///     Initializes a new instance of the GraphQLClient with the specified endpoint configuration.
    /// </summary>
    /// <param name="endpointConfiguration">The GraphQL endpoint configuration.</param>
    public GraphQLClient(GraphQLEndpointConfiguration endpointConfiguration)
        : base(endpointConfiguration)
    {
        Name = nameof(GraphQLClient);
        _correlationManager =
            new PollingCorrelationManager<IMessage>(endpointConfiguration, "GraphQL reply message did not arrive yet");
    }

    /// <summary>
    ///     Default constructor initializing endpoint configuration.
    /// </summary>
    public GraphQLClient() : this(new GraphQLEndpointConfiguration())
    {
    }

    /// <summary>
    ///     Gets the GraphQL endpoint configuration for this client.
    /// </summary>
    public override GraphQLEndpointConfiguration EndpointConfiguration =>
        (GraphQLEndpointConfiguration)base.EndpointConfiguration;

    /// <summary>
    ///     Sends a GraphQL message and stores the response for later retrieval.
    /// </summary>
    /// <param name="message">The GraphQL message to send.</param>
    /// <param name="context">The test context.</param>
    public void Send(IMessage message, TestContext context)
    {
        // Configure message listeners for logging handlers
        foreach (var interceptor in EndpointConfiguration.ClientHandlers
                     .OfType<LoggingGraphQLClientHandler>()
                     .Where(interceptor => !interceptor.HasMessageListeners()))
        {
            interceptor.SetMessageListener(context.MessageListeners);
        }

        GraphQLMessage graphQLMessage;
        if (message is GraphQLMessage gqlMsg)
        {
            graphQLMessage = gqlMsg;
        }
        else
        {
            graphQLMessage = new GraphQLMessage(message);
        }

        var correlationKeyName = GetCorrelationKeyName();
        var correlationKey = EndpointConfiguration.Correlator.GetCorrelationKey(graphQLMessage);
        _correlationManager.SaveCorrelationKey(correlationKeyName, correlationKey, context);

        var endpointUri = GetEndpointUri(graphQLMessage);
        context.SetVariable(MessageHeaders.MessageReplyTo + "_" + correlationKeyName, endpointUri);

        Log.LogInformation("Sending GraphQL message to: '{EndpointUri}'", endpointUri);
        Log.LogDebug("GraphQL message to send:\n{Payload}", graphQLMessage.GetPayload<string>());

        try
        {
            // Validate configuration
            EndpointConfiguration.Validate();

            // Convert message to GraphQL request
            var graphQLRequest = EndpointConfiguration.MessageConverter.ConvertOutbound(
                graphQLMessage, EndpointConfiguration, context);

            // Determine an operation type to decide an execution path
            var operationType = DetermineOperationType(graphQLMessage.GetPayload<string>());

            if (operationType.Equals(nameof(GraphQLOperationType.SUBSCRIPTION),
                    StringComparison.InvariantCultureIgnoreCase))
            {
                // Handle subscription - store first response and set up stream
                HandleSubscriptionInSend(graphQLRequest, graphQLMessage, context, correlationKey);
            }
            else
            {
                // Handle query/mutation - execute and store response
                var response = ExecuteGraphQLRequestAsync(graphQLRequest, graphQLMessage, context).GetAwaiter()
                    .GetResult();
                var responseMessage =
                    EndpointConfiguration.MessageConverter.ConvertInbound(response, EndpointConfiguration, context);

                // Store response for correlation
                _correlationManager.Store(correlationKey, responseMessage);
            }

            Log.LogDebug("GraphQL message was sent to endpoint: '{EndpointUri}'", endpointUri);
        }
        catch (Exception ex)
        {
            Log.LogWarning(ex, "Caught GraphQL Client exception!");
        }
    }

    /// <summary>
    ///     Receives a message using a correlation key derived from the context.
    /// </summary>
    /// <param name="context">The test context providing additional information required for receiving the message.</param>
    /// <returns>Returns an <see cref="IMessage" /> instance that matches the correlation key.</returns>
    public IMessage Receive(TestContext context)
    {
        return Receive(_correlationManager.GetCorrelationKey(GetCorrelationKeyName(), context), context);
    }

    /// <summary>
    ///     Receives a message from the endpoint using a specified correlation key and context, with a timeout.
    /// </summary>
    /// <param name="context">The context in which the message is being received.</param>
    /// <param name="timeout">The time in milliseconds to wait for the message before timing out.</param>
    /// <returns>The received message, or null if the timeout expires before a message is received.</returns>
    public virtual IMessage Receive(TestContext context, long timeout)
    {
        return Receive(_correlationManager.GetCorrelationKey(GetCorrelationKeyName(), context), context, timeout);
    }

    /// <summary>
    ///     Receives a message from the endpoint using the provided selector and test context.
    /// </summary>
    /// <param name="selector">The correlation key used to identify the message.</param>
    /// <param name="context">The test context which may contain details for message correlation.</param>
    /// <returns>The received message as an <see cref="IMessage" />.</returns>
    public IMessage Receive(string selector, TestContext context)
    {
        return Receive(selector, context, EndpointConfiguration.Timeout);
    }

    /// <summary>
    ///     Receives a message based on the specified selector within a given timeout period.
    /// </summary>
    /// <param name="selector">The selector criteria to filter the messages.</param>
    /// <param name="context">The test context that provides necessary execution environment information.</param>
    /// <param name="timeout">The maximum amount of time to wait for a message before timing out.</param>
    /// <returns>The message that matches the selector criteria.</returns>
    /// <exception cref="MessageTimeoutException">Thrown when a message cannot be received within the specified timeout.</exception>
    public IMessage Receive(string selector, TestContext context, long timeout)
    {
        var message = _correlationManager.Find(selector, timeout);

        var endpointUri = context.GetVariables().ContainsKey(MessageHeaders.MessageReplyTo + "_" + selector)
            ? context.GetVariable(MessageHeaders.MessageReplyTo + "_" + selector)
            : Name;

        if (message == null)
        {
            throw new MessageTimeoutException(timeout, endpointUri);
        }

        return message;
    }

    /// <summary>
    ///     Handles subscription execution within the Send method to ensure responses are stored for Receive methods.
    /// </summary>
    /// <param name="graphQLRequest">The GraphQL request.</param>
    /// <param name="graphQLMessage">The original GraphQL message.</param>
    /// <param name="context">The test context.</param>
    /// <param name="correlationKey">The correlation key for storing responses.</param>
    private void HandleSubscriptionInSend(GraphQLRequest graphQLRequest, GraphQLMessage graphQLMessage,
        TestContext context, string correlationKey)
    {
        var subscriptionId = Guid.NewGuid().ToString();

        // Create subscription stream
        var subscriptionStream = CreateSubscriptionStream(graphQLRequest, graphQLMessage, context, subscriptionId);

        lock (_subscriptionLock)
        {
            _activeSubscriptions[subscriptionId] = subscriptionStream;
        }

        var sequenceNumber = 0;

        // Subscribe to the stream and store responses for Receive methods
        subscriptionStream
            .Select(response => EndpointConfiguration.MessageConverter.ConvertInbound(
                response, EndpointConfiguration, context))
            .Subscribe(
                responseMessage =>
                {
                    // Use unique correlation key for each response
                    var sequenceKey = $"{correlationKey}_seq_{Interlocked.Increment(ref sequenceNumber)}";
                    _correlationManager.Store(sequenceKey, responseMessage);

                    // ALSO store as latest response with base correlation key
                    _correlationManager.Store(correlationKey, responseMessage);
                },
                ex =>
                {
                    Log.LogError(ex, "GraphQL subscription error");
                    lock (_subscriptionLock)
                    {
                        _activeSubscriptions.Remove(subscriptionId);
                    }
                },
                () =>
                {
                    Log.LogDebug("GraphQL subscription completed");
                    lock (_subscriptionLock)
                    {
                        _activeSubscriptions.Remove(subscriptionId);
                    }
                });
    }

    /// <summary>
    ///     Creates and returns an instance of a producer.
    /// </summary>
    /// <returns>The current instance as an implementation of the IProducer interface.</returns>
    public override IProducer CreateProducer()
    {
        return this;
    }

    /// <summary>
    ///     Creates and returns an instance of an ISelectiveConsumer.
    /// </summary>
    /// <returns>An instance of ISelectiveConsumer, specifically the current instance of GraphQLClient.</returns>
    public override ISelectiveConsumer CreateConsumer()
    {
        return this;
    }

    /// <summary>
    ///     Sets the correlation manager used for managing message correlation.
    /// </summary>
    /// <param name="correlationManager">The correlation manager to be set for message correlation operations.</param>
    public void SetCorrelationManager(ICorrelationManager<IMessage> correlationManager)
    {
        _correlationManager = correlationManager;
    }

    /// <summary>
    ///     Executes a GraphQL request and handles retries according to the configured policy.
    /// </summary>
    /// <param name="request">The GraphQL request to execute.</param>
    /// <param name="originalMessage">The original GraphQL message.</param>
    /// <param name="context">The test context.</param>
    /// <returns>The GraphQL response.</returns>
    private async Task<GraphQLResponse<object>> ExecuteGraphQLRequestAsync(
        GraphQLRequest request, GraphQLMessage originalMessage, TestContext context)
    {
        var retryPolicy = EndpointConfiguration.RetryPolicy;
        var currentAttempt = 0;
        Exception? lastException = null;

        while (currentAttempt <= retryPolicy.MaxRetries)
        {
            try
            {
                // Set retry attempt header
                if (currentAttempt > 0)
                {
                    originalMessage.SetHeader(GraphQLMessageHeaders.RetryAttempt, currentAttempt.ToString());
                }

                // Execute the request
                var response = await EndpointConfiguration.GraphQLClient.SendQueryAsync<object>(request);

                // Check for GraphQL errors that might warrant a retry
                if (ShouldRetryOnGraphQLErrors(response, retryPolicy))
                {
                    throw new AgenixSystemException(
                        $"GraphQL errors: {string.Join(", ", response.Errors.Select(e => e.Message))}");
                }

                return response;
            }
            catch (Exception ex)
            {
                lastException = ex;
                currentAttempt++;

                if (currentAttempt > retryPolicy.MaxRetries || !ShouldRetryOnException(ex, retryPolicy))
                {
                    break;
                }

                // Calculate delay with optional exponential backoff
                var delay = retryPolicy.UseExponentialBackoff
                    ? retryPolicy.RetryDelayMilliseconds * (int)Math.Pow(2, currentAttempt - 1)
                    : retryPolicy.RetryDelayMilliseconds;

                Log.LogWarning(
                    "GraphQL request failed (attempt {Attempt}/{MaxAttempts}), retrying in {Delay}ms: {Error}",
                    currentAttempt, retryPolicy.MaxRetries + 1, delay, ex.Message);

                await Task.Delay(delay);
            }
        }

        throw new AgenixSystemException($"GraphQL request failed after {retryPolicy.MaxRetries + 1} attempts",
            lastException);
    }

    /// <summary>
    ///     Creates a subscription stream for GraphQL subscriptions.
    /// </summary>
    /// <param name="request">The GraphQL subscription request.</param>
    /// <param name="originalMessage">The original GraphQL message.</param>
    /// <param name="context">The test context.</param>
    /// <param name="subscriptionId">The subscription identifier.</param>
    /// <returns>Observable stream of GraphQL responses.</returns>
    private IObservable<GraphQLResponse<object>> CreateSubscriptionStream(
        GraphQLRequest request, GraphQLMessage originalMessage, TestContext context, string subscriptionId)
    {
        if (!EndpointConfiguration.UseWebSocketForSubscriptions)
        {
            throw new NotSupportedException("WebSocket subscriptions are disabled in configuration");
        }

        // Set subscription headers
        originalMessage.SetHeader(GraphQLMessageHeaders.SubscriptionConnectionId, subscriptionId);
        originalMessage.SetHeader(GraphQLMessageHeaders.UseWebSocket, "true");

        return EndpointConfiguration.GraphQLClient.CreateSubscriptionStream<object>(request)
            .Timeout(TimeSpan.FromMilliseconds(EndpointConfiguration.Timeout))
            .Retry(EndpointConfiguration.RetryPolicy.MaxRetries);
    }

    /// <summary>
    ///     Retrieves the endpoint URI for the given GraphQL message, using the configured endpoint URI resolver if available.
    /// </summary>
    /// <param name="graphQLMessage">The GraphQL message for which the endpoint URI needs to be determined.</param>
    /// <returns>The resolved endpoint URI if the resolver is available, otherwise the default endpoint URL.</returns>
    private string? GetEndpointUri(GraphQLMessage graphQLMessage)
    {
        return EndpointConfiguration.EndpointUriResolver?.ResolveEndpointUri(graphQLMessage,
            EndpointConfiguration.EndpointUrl);
    }

    /// <summary>
    ///     Retrieves the correlation key name using the configured correlator and the current endpoint name.
    /// </summary>
    /// <returns>The correlation key name derived from the consumer name associated with the current endpoint.</returns>
    private string GetCorrelationKeyName()
    {
        return EndpointConfiguration.Correlator.GetCorrelationKeyName(Name);
    }

    /// <summary>
    ///     Determines if a retry should be attempted based on GraphQL errors.
    /// </summary>
    /// <param name="response">The GraphQL response.</param>
    /// <param name="retryPolicy">The retry policy configuration.</param>
    /// <returns>True if retry should be attempted.</returns>
    private static bool ShouldRetryOnGraphQLErrors(GraphQLResponse<object> response,
        GraphQLEndpointConfiguration.GraphQLRetryPolicy retryPolicy)
    {
        if (response.Errors == null || response.Errors.Length == 0)
        {
            return false;
        }

        // Retry on specific GraphQL error types (network, timeout, etc.)
        return response.Errors.Any(error =>
            error.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
            error.Message.Contains("network", StringComparison.OrdinalIgnoreCase) ||
            error.Message.Contains("connection", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    ///     Determines if a retry should be attempted based on the exception.
    /// </summary>
    /// <param name="exception">The exception that occurred.</param>
    /// <param name="retryPolicy">The retry policy configuration.</param>
    /// <returns>True if retry should be attempted.</returns>
    private static bool ShouldRetryOnException(Exception exception,
        GraphQLEndpointConfiguration.GraphQLRetryPolicy retryPolicy)
    {
        return exception is HttpRequestException ||
               exception is TaskCanceledException ||
               exception is TimeoutException ||
               (exception is GraphQLHttpRequestException graphQLEx &&
                retryPolicy.RetryableStatusCodes.Contains((int)graphQLEx.StatusCode));
    }

    /// <summary>
    ///     Determines the operation type from a GraphQL query string.
    /// </summary>
    /// <param name="query">The GraphQL query string.</param>
    /// <returns>The operation type (Query, Mutation, Subscription).</returns>
    private static string DetermineOperationType(string? query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return "Query";
        }

        var trimmedQuery = query.Trim();

        if (trimmedQuery.StartsWith(nameof(GraphQLOperationType.MUTATION), StringComparison.OrdinalIgnoreCase))
        {
            return "Mutation";
        }

        return trimmedQuery.StartsWith(nameof(GraphQLOperationType.SUBSCRIPTION), StringComparison.OrdinalIgnoreCase)
            ? "Subscription"
            : "Query";
    }
}
