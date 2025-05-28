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

using Agenix.Api;
using Agenix.Api.Spi;
using Agenix.Core.Util;
using HttpClient = Agenix.Http.Client.HttpClient;

namespace Agenix.Http.Actions;

/// Provides a fluent API for building and configuring HTTP actions.
/// Supports initiating client or server HTTP operations.
/// Extends behavior to resolve references dynamically during action construction.
/// /
public class HttpActionBuilder : AbstractReferenceResolverAwareTestActionBuilder<ITestAction>
{
    /// Static entrance method for the HTTP fluent action builder.
    /// @return The HTTP action builder instance.
    /// /
    public static HttpActionBuilder Http()
    {
        return new HttpActionBuilder();
    }

    /// Initiates an HTTP client action for use with the specified HTTP client.
    /// Dynamically builds a client-side HTTP action, allowing customization of headers,
    /// endpoints, and payload handling through a fluent API.
    /// <param name="httpClient">The HTTP client to be used for this action.</param>
    /// <returns>A builder instance for further configuration of the client action.</returns>
    public HttpClientActionBuilder Client(HttpClient httpClient)
    {
        var clientActionBuilder = new HttpClientActionBuilder(httpClient)
            .WithReferenceResolver(referenceResolver);
        _delegate = clientActionBuilder;
        return clientActionBuilder;
    }

    /// Initializes and returns an HTTP client action builder for configuring
    /// HTTP client operations with the specified HTTP client identifier.
    /// <param name="httpClient">The identifier of the HTTP client used to construct the action builder.</param>
    /// <returns>An instance of HttpClientActionBuilder to configure and build HTTP client actions.</returns>
    public HttpClientActionBuilder Client(string httpClient)
    {
        var clientActionBuilder = new HttpClientActionBuilder(httpClient)
            .WithReferenceResolver(referenceResolver);
        _delegate = clientActionBuilder;
        return clientActionBuilder;
    }

    /// Sets the bean reference resolver to be used for resolving references during the building process.
    /// <param name="referenceResolver">The reference resolver instance to set.</param>
    /// <return>This instance of HttpActionBuilder for method chaining.</return>
    public HttpActionBuilder WithReferenceResolver(IReferenceResolver referenceResolver)
    {
        this.referenceResolver = referenceResolver;
        return this;
    }

    public override ITestAction Build()
    {
        ObjectHelper.AssertNotNull(_delegate, "Missing delegate action to build");
        return _delegate.Build();
    }
}
