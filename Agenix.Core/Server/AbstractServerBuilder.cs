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


using Agenix.Api.Endpoint;
using Agenix.Core.Endpoint;

namespace Agenix.Core.Server;

/// <summary>
/// Abstract server builder providing fluent API for configuring server instances.
/// </summary>
/// <typeparam name="TServer">The server type that extends AbstractServer.</typeparam>
/// <typeparam name="TBuilder">The builder type that extends AbstractServerBuilder.</typeparam>
public abstract class AbstractServerBuilder<TServer, TBuilder> : AbstractEndpointBuilder<TServer>
    where TServer : AbstractServer
    where TBuilder : AbstractServerBuilder<TServer, TBuilder>
{
    private readonly TBuilder _self;

    protected AbstractServerBuilder()
    {
        _self = (TBuilder)this;
    }

    /// <summary>
    /// Sets the autoStart property.
    /// </summary>
    /// <param name="autoStart">Whether to auto-start the server.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TBuilder AutoStart(bool autoStart)
    {
        GetEndpoint().AutoStart = autoStart;
        return _self;
    }

    /// <summary>
    /// Sets the endpoint adapter.
    /// </summary>
    /// <param name="endpointAdapter">The endpoint adapter to set.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TBuilder EndpointAdapter(IEndpointAdapter endpointAdapter)
    {
        GetEndpoint().EndpointAdapter = endpointAdapter;
        return _self;
    }

    /// <summary>
    /// Sets the debug logging enabled flag.
    /// </summary>
    /// <param name="enabled">Whether debug logging is enabled.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public TBuilder DebugLogging(bool enabled)
    {
        GetEndpoint().DebugLogging = enabled;
        return _self;
    }

    /// <summary>
    /// Sets the default timeout.
    /// </summary>
    /// <param name="timeout">The timeout value in milliseconds.</param>
    /// <returns>The builder instance for method chaining.</returns>
    public virtual TBuilder Timeout(long timeout)
    {
        if (GetEndpoint()?.EndpointConfiguration != null)
        {
            GetEndpoint().EndpointConfiguration.Timeout = timeout;
        }

        GetEndpoint().DefaultTimeout = timeout;
        return _self;
    }
}
