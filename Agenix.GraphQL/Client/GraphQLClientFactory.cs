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

namespace Agenix.GraphQL.Client;

/// <summary>
///     Represents a factory for creating message handlers for GraphQL operations.
///     This factory allows the configuration of multiple delegating handlers that form a pipeline
///     for processing GraphQL HTTP requests and responses.
/// </summary>
public class GraphQLClientFactory
{
    private readonly List<DelegatingHandler> _clientHandlers = [];

    /// <summary>
    ///     Creates a message handler pipeline that can be used by GraphQL clients.
    ///     This is the primary method used for GraphQL client configuration.
    /// </summary>
    /// <returns>A configured HttpMessageHandler for use with GraphQLHttpClient.</returns>
    public HttpMessageHandler CreateMessageHandler()
    {
        return CreateHandlerPipeline(new HttpClientHandler(), _clientHandlers);
    }

    /// <summary>
    ///     Adds a DelegatingHandler to the collection of handlers within the GraphQLClientFactory.
    ///     This handler will be part of the pipeline used by the GraphQL clients created by the factory.
    /// </summary>
    /// <param name="handler">The DelegatingHandler instance to be added to the collection of handlers.</param>
    /// <returns>The modified GraphQLClientFactory instance, allowing for method chaining.</returns>
    public GraphQLClientFactory AddHandler(DelegatingHandler handler)
    {
        _clientHandlers.Add(handler);
        return this;
    }

    /// <summary>
    ///     Adds a collection of DelegatingHandler instances to the existing set of handlers in the GraphQLClientFactory.
    ///     These handlers will be incorporated into the request processing pipeline of GraphQL clients created by the
    ///     factory.
    /// </summary>
    /// <param name="handlers">The list of DelegatingHandler instances to be added to the collection.</param>
    /// <returns>The modified GraphQLClientFactory instance, allowing for method chaining.</returns>
    public GraphQLClientFactory AddHandlers(List<DelegatingHandler> handlers)
    {
        _clientHandlers.AddRange(handlers);
        return this;
    }

    /// <summary>
    ///     Creates a handler pipeline by chaining a collection of DelegatingHandler instances with a specified inner handler.
    ///     It constructs the pipeline by setting each handler's InnerHandler property to the next handler in the sequence,
    ///     with the specified inner handler serving as the final handler in the chain.
    /// </summary>
    /// <param name="innerHandler">
    ///     The innermost handler, typically an instance of <see cref="HttpMessageHandler" />, which
    ///     processes the HTTP response and request.
    /// </param>
    /// <param name="handlers">An enumerable collection of DelegatingHandler instances to be chained together in the pipeline.</param>
    /// <returns>
    ///     A <see cref="HttpMessageHandler" /> that represents the completed handler chain, starting with the first
    ///     handler in the collection and ending with the specified inner handler.
    /// </returns>
    private HttpMessageHandler CreateHandlerPipeline(HttpMessageHandler innerHandler,
        IEnumerable<DelegatingHandler> handlers)
    {
        var current = innerHandler;
        foreach (var handler in handlers)
        {
            handler.InnerHandler = current;
            current = handler;
        }

        return current;
    }
}
