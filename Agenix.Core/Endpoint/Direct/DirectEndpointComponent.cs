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
            context.ReferenceResolver.Bind(queueName, new DefaultMessageQueue(queueName));

        EnrichEndpointConfiguration(endpoint.EndpointConfiguration, parameters, context);

        return endpoint;
    }
}