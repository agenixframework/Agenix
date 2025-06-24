#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
// 
//   http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
// 
// Copyright (c) 2025 Agenix
// 
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

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
