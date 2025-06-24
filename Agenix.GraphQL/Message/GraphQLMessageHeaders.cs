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

namespace Agenix.GraphQL.Message;

/// <summary>
///     Contains constants for GraphQL-specific message headers.
/// </summary>
public static class GraphQLMessageHeaders
{
    /// <summary>
    ///     The prefix used for all GraphQL-specific headers.
    /// </summary>
    public const string GraphQLPrefix = "agenix_graphql_";

    /// <summary>
    ///     Content-Type header for GraphQL requests.
    /// </summary>
    public const string ContentType = "Content-Type";

    /// <summary>
    ///     Accept header for GraphQL requests.
    /// </summary>
    public const string Accept = "Accept";

    /// <summary>
    ///     Authorization header for GraphQL requests.
    /// </summary>
    public const string Authorization = "Authorization";

    /// <summary>
    ///     Header containing the GraphQL operation type.
    /// </summary>
    public const string OperationType = GraphQLPrefix + "operation_type";

    /// <summary>
    ///     Header containing the GraphQL operation name.
    /// </summary>
    public const string OperationName = GraphQLPrefix + "operation_name";

    /// <summary>
    ///     Header containing the GraphQL endpoint URL.
    /// </summary>
    public const string EndpointUrl = GraphQLPrefix + "endpoint_url";

    /// <summary>
    ///     Header indicating whether to use the GET method for queries.
    /// </summary>
    public const string UseGetForQueries = GraphQLPrefix + "use_get_for_queries";

    /// <summary>
    ///     Header containing the GraphQL query hash for persisted queries.
    /// </summary>
    public const string QueryHash = GraphQLPrefix + "query_hash";

    /// <summary>
    /// Indicates whether WebSocket should be used for subscriptions.
    /// </summary>
    public static readonly string UseWebSocket = GraphQLPrefix + "use_websocket";

    /// <summary>
    /// The retry attempt number for failed requests.
    /// </summary>
    public static readonly string RetryAttempt = GraphQLPrefix + "retry_attempt";

    /// <summary>
    ///     Header containing the GraphQL subscription ID.
    /// </summary>
    public const string SubscriptionId = GraphQLPrefix + "subscription_id";

    /// <summary>
    ///     Header containing the GraphQL request timeout.
    /// </summary>
    public const string RequestTimeout = GraphQLPrefix + "request_timeout";

    /// <summary>
    ///     Header containing the GraphQL client version.
    /// </summary>
    public const string ClientVersion = GraphQLPrefix + "client_version";

    /// <summary>
    ///     Header containing custom GraphQL extensions.
    /// </summary>
    public const string Extensions = GraphQLPrefix + "extensions";

    /// <summary>
    ///     Indicates whether the response contains GraphQL errors.
    /// </summary>
    public static readonly string HasErrors = GraphQLPrefix + "has_errors";

    /// <summary>
    ///     The number of errors in the GraphQL response.
    /// </summary>
    public static readonly string ErrorCount = GraphQLPrefix + "error_count";

    /// <summary>
    ///     Concatenated error messages from the GraphQL response.
    /// </summary>
    public static readonly string ErrorMessages = GraphQLPrefix + "error_messages";

    /// <summary>
    ///     Indicates a parse error occurred when processing the response.
    /// </summary>
    public static readonly string ParseError = GraphQLPrefix + "parse_error";

    /// <summary>
    ///     The character set used for encoding.
    /// </summary>
    public static readonly string Charset = GraphQLPrefix + "charset";

    /// <summary>
    ///     The HTTP status code of the response.
    /// </summary>
    public static readonly string StatusCode = GraphQLPrefix + "status_code";

    /// Defines the key for the HTTP reason phrase header in HTTP message headers.
    /// This header provides a short textual description associated with the HTTP status code,
    /// typically sent by servers in HTTP response messages.
    public static readonly string ReasonPhrase = GraphQLPrefix + "reason_phrase";

    /// <summary>
    ///     The HTTP status text of the response.
    /// </summary>
    public static readonly string StatusText = GraphQLPrefix + "status_text";

    /// <summary>
    /// The subscription connection ID for WebSocket subscriptions.
    /// </summary>
    public static readonly string SubscriptionConnectionId = GraphQLPrefix + "subscription_connection_id";
}
