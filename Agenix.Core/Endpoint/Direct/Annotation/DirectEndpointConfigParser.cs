using System;
using Agenix.Api.Config.Annotation;
using Agenix.Api.Message;
using Agenix.Core.Spi;

namespace Agenix.Core.Endpoint.Direct.Annotation;

/// <summary>
///     DirectEndpointConfigParser is responsible for parsing DirectEndpointConfigAttribute
///     instances to create and configure DirectEndpoint objects.
/// </summary>
public class DirectEndpointConfigParser : IAnnotationConfigParser<DirectEndpointConfigAttribute, DirectEndpoint>
{
    /// <summary>
    ///     Parses a given DirectEndpointConfigAttribute to create a DirectEndpoint instance.
    /// </summary>
    /// <param name="annotation">The DirectEndpointConfigAttribute containing configuration for the DirectEndpoint.</param>
    /// <param name="referenceResolver">The IReferenceResolver instance used to resolve references during parsing.</param>
    /// <returns>A DirectEndpoint instance configured based on the provided annotation.</returns>
    public DirectEndpoint Parse(DirectEndpointConfigAttribute annotation, IReferenceResolver referenceResolver)
    {
        var builder = new DirectEndpointBuilder();

        var queue = annotation.Queue;
        var queueName = annotation.QueueName;

        if (!string.IsNullOrWhiteSpace(queue))
            builder.Queue(referenceResolver.Resolve<IMessageQueue>(annotation.Queue));

        if (!string.IsNullOrWhiteSpace(queueName)) builder.Queue(annotation.QueueName);

        builder.Timeout(annotation.Timeout);

        return builder.Initialize().Build();
    }

    /// <summary>
    ///     Parses a given DirectEndpointConfigAttribute to create a DirectEndpoint instance.
    /// </summary>
    /// <param name="annotation">The DirectEndpointConfigAttribute containing configuration for the DirectEndpoint.</param>
    /// <param name="referenceResolver">The IReferenceResolver instance used to resolve references during parsing.</param>
    /// <returns>A DirectEndpoint instance configured based on the provided annotation.</returns>
    object IAnnotationConfigParser.Parse(Attribute annotation, IReferenceResolver referenceResolver)
    {
        return Parse(annotation as DirectEndpointConfigAttribute, referenceResolver);
    }
}