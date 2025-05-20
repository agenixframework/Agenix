using System;
using Agenix.Api.Config.Annotation;
using Agenix.Api.Message;
using Agenix.Core.Message;
using Agenix.Core.Spi;

namespace Agenix.Core.Endpoint.Direct.Annotation;

/// <summary>
///     DirectSyncEndpointConfigParser is responsible for parsing DirectSyncEndpointConfigAttribute annotations
///     and resolving references to generate a configured DirectSyncEndpoint instance.
/// </summary>
public class
    DirectSyncEndpointConfigParser : IAnnotationConfigParser<DirectSyncEndpointConfigAttribute, DirectSyncEndpoint>
{
    /// <summary>
    ///     Parses the provided DirectSyncEndpointConfigAttribute and resolves any references
    ///     using the specified IReferenceResolver to create a DirectSyncEndpoint instance.
    /// </summary>
    /// <param name="annotation">The DirectSyncEndpointConfigAttribute containing the configuration data.</param>
    /// <param name="referenceResolver">The reference resolver to resolve any dependencies from the configuration.</param>
    /// <returns>A DirectSyncEndpoint instance populated with the configuration data.</returns>
    public DirectSyncEndpoint Parse(DirectSyncEndpointConfigAttribute annotation, IReferenceResolver referenceResolver)
    {
        var builder = new DirectSyncEndpointBuilder();

        var queue = annotation.Queue;
        var queueName = annotation.QueueName;

        if (!string.IsNullOrWhiteSpace(queue))
            builder.Queue(referenceResolver.Resolve<IMessageQueue>(annotation.Queue));

        if (!string.IsNullOrWhiteSpace(queueName)) builder.Queue(annotation.QueueName);

        builder.Timeout(annotation.Timeout);

        if (!string.IsNullOrWhiteSpace(annotation.Correlator))
            builder.Correlator(referenceResolver.Resolve<IMessageCorrelator>(annotation.Correlator));

        builder.PollingInterval(annotation.PollingInterval);

        return builder.Initialize().Build();
    }

    /// <summary>
    ///     Parses the provided DirectSyncEndpointConfigAttribute and resolves any references
    ///     using the specified IReferenceResolver to create a DirectSyncEndpoint instance.
    /// </summary>
    /// <param name="annotation">The DirectSyncEndpointConfigAttribute containing the configuration data.</param>
    /// <param name="referenceResolver">The reference resolver to resolve any dependencies from the configuration.</param>
    /// <returns>A DirectSyncEndpoint instance populated with the configuration data.</returns>
    object IAnnotationConfigParser.Parse(Attribute annotation, IReferenceResolver referenceResolver)
    {
        return Parse(annotation as DirectSyncEndpointConfigAttribute, referenceResolver);
    }
}