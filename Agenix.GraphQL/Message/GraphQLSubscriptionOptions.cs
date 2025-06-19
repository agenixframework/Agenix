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

namespace Agenix.GraphQL.Message;

/// <summary>
///     Represents configuration options for GraphQL subscriptions.
/// </summary>
public class GraphQLSubscriptionOptions
{
    /// <summary>
    ///     Gets or sets the maximum duration to keep the subscription alive.
    /// </summary>
    public TimeSpan? MaxDuration { get; set; }

    /// <summary>
    ///     Gets or sets whether to automatically reconnect on connection loss.
    /// </summary>
    public bool AutoReconnect { get; set; } = true;

    /// <summary>
    ///     Gets or sets the reconnection delay.
    /// </summary>
    public TimeSpan ReconnectDelay { get; set; } = TimeSpan.FromSeconds(5);

    /// <summary>
    ///     Gets or sets the maximum number of reconnection attempts.
    /// </summary>
    public int MaxReconnectAttempts { get; set; } = 3;

    /// <summary>
    ///     Gets or sets the WebSocket sub-protocol to use.
    /// </summary>
    public string? WebSocketSubProtocol { get; set; } = "graphql-ws";

    /// <summary>
    ///     Gets or sets additional connection parameters for the subscription.
    /// </summary>
    public Dictionary<string, object>? ConnectionParams { get; set; }

    /// <summary>
    ///     Gets or sets the heartbeat interval for keeping the connection alive.
    /// </summary>
    public TimeSpan? HeartbeatInterval { get; set; } = TimeSpan.FromSeconds(30);

    /// <summary>
    ///     Gets or sets whether to buffer messages when the connection is temporarily unavailable.
    /// </summary>
    public bool BufferMessages { get; set; } = true;

    /// <summary>
    ///     Gets or sets the maximum number of messages to buffer.
    /// </summary>
    public int MaxBufferSize { get; set; } = 1000;
}
