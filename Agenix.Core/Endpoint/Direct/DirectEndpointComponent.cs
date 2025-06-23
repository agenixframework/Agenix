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

using System.Collections.Generic;
using Agenix.Api.Context;
using Agenix.Api.Endpoint;
using Agenix.Core.Message;

namespace Agenix.Core.Endpoint.Direct;

public class DirectEndpointComponent(string name = "direct") : AbstractEndpointComponent(name)
{
    public DirectEndpointComponent() : this("direct")
    {
    }

    /// <summary>
    ///     Creates an endpoint for the given resource path, parameters, and context.
    /// </summary>
    /// <param name="resourcePath">The resource path that determines the type and configuration of the endpoint.</param>
    /// <param name="parameters">A dictionary of parameters used to further configure the endpoint.</param>
    /// <param name="context">The test context that provides additional configuration and reference resolution.</param>
    /// <returns>Returns an instance of an object that implements the IEndpoint interface.</returns>
    protected override IEndpoint CreateEndpoint(string resourcePath, IDictionary<string, string> parameters,
        TestContext context)
    {
        DirectEndpoint endpoint;
        string queueName;

        if (resourcePath.StartsWith("sync:"))
        {
            var endpointConfiguration = new DirectSyncEndpointConfiguration();
            endpoint = new DirectSyncEndpoint(endpointConfiguration);
            queueName = resourcePath["sync:".Length..];
        }
        else
        {
            endpoint = new DirectEndpoint();
            queueName = resourcePath;
        }

        endpoint.EndpointConfiguration.SetQueueName(queueName);

        if (!context.ReferenceResolver.IsResolvable(queueName))
        {
            context.ReferenceResolver.Bind(queueName, new DefaultMessageQueue(queueName));
        }

        EnrichEndpointConfiguration(endpoint.EndpointConfiguration, parameters, context);

        return endpoint;
    }
}
