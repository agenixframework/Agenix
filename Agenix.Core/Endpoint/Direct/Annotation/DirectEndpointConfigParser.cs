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
