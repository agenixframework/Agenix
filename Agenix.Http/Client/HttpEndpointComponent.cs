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

using Agenix.Api.Context;
using Agenix.Api.Endpoint;
using Agenix.Core.Endpoint;

namespace Agenix.Http.Client;

/// Represents a component responsible for creating HTTP client endpoints based on a given URI
/// resource and associated parameters. It supports configuration of HTTP-specific endpoint settings.
/// /
public class HttpEndpointComponent : AbstractEndpointComponent
{
    /// Represents a component responsible for creating HTTP client endpoints based on a given URI resource
    /// and associated parameters. It allows configuration of HTTP-specific endpoint settings and supports
    /// various HTTP-related options such as request methods and error handling strategies.
    /// /
    public HttpEndpointComponent() : this("http")
    {
    }

    /// Represents a specialized component used for creating HTTP client endpoints based on specific
    /// resource paths and parameters. This component handles configuration and initialization of
    /// HTTP endpoints, enabling customization of options such as request methods and error handling.
    public HttpEndpointComponent(string name) : base(name)
    {
    }

    /// Gets the URI scheme used for HTTP endpoints.
    /// @return A string representing the scheme (e.g., "http://").
    /// /
    protected virtual string Scheme => "http://";

    /// Creates an HTTP endpoint based on the provided resource path, parameters, and execution context.
    /// This method constructs an endpoint with configuration details defined in the parameters and
    /// customizes specific HTTP options such as the request method and URL structure.
    /// <param name="resourcePath">The relative path for the resource to be accessed by the HTTP endpoint.</param>
    /// <param name="parameters">A dictionary of parameters used to configure the endpoint, including HTTP-specific options.</param>
    /// <param name="context">The context for the test environment, providing configuration or dependencies.</param>
    /// <return>An instance of IEndpoint configured as an HTTP client endpoint.</return>
    protected override IEndpoint CreateEndpoint(string resourcePath, IDictionary<string, string> parameters,
        TestContext context)
    {
        var client = new HttpClient
        {
            EndpointConfiguration =
            {
                RequestUrl = Scheme + resourcePath +
                             GetParameterString(parameters, typeof(HttpEndpointConfiguration))
            }
        };


        if (parameters.Remove("requestMethod", out var value))
        {
            client.EndpointConfiguration.RequestMethod = HttpMethod.Parse(value);
        }


        EnrichEndpointConfiguration(
            client.EndpointConfiguration,
            GetEndpointConfigurationParameters(parameters, typeof(HttpEndpointConfiguration)),
            context);
        return client;
    }
}
