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

using System;
using Agenix.Api.Config.Annotation;
using Agenix.Api.Message;
using Agenix.Api.Spi;

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
        {
            builder.Queue(referenceResolver.Resolve<IMessageQueue>(annotation.Queue));
        }

        if (!string.IsNullOrWhiteSpace(queueName))
        {
            builder.Queue(annotation.QueueName);
        }

        builder.Timeout(annotation.Timeout);

        if (!string.IsNullOrWhiteSpace(annotation.Correlator))
        {
            builder.Correlator(referenceResolver.Resolve<IMessageCorrelator>(annotation.Correlator));
        }

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
