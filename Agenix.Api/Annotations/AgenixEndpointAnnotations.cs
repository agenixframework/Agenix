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

using System.Reflection;
using Agenix.Api.Context;
using Agenix.Api.Endpoint;
using Agenix.Api.Log;
using Agenix.Api.Spi;
using Agenix.Api.Util;
using Microsoft.Extensions.Logging;

namespace Agenix.Api.Annotations;

/// <summary>
///     Dependency injection support for AgenixEndpoint endpoint annotations.
/// </summary>
public abstract class AgenixEndpointAnnotations
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(AgenixEndpointAnnotations));

    /// <summary>
    ///     Prevent instantiation.
    /// </summary>
    private AgenixEndpointAnnotations()
    {
    }

    /// <summary>
    ///     Reads all AgenixEndpoint and AgenixEndpointConfig related annotations on target object field declarations and
    ///     injects proper endpoint instances.
    /// </summary>
    /// <param name="target"></param>
    /// <param name="context"></param>
    public static void InjectEndpoints(object target, TestContext context)
    {
        ReflectionHelper.DoWithFields(target.GetType(), field =>
        {
            if (!field.IsDefined(typeof(AgenixEndpointAttribute)) ||
                !typeof(IEndpoint).IsAssignableFrom(field.FieldType))
            {
                return;
            }

            Log.LogDebug("Injecting Agenix endpoint on test class field '{FieldName}'", field.Name);
            var endpointAnnotation = field.GetCustomAttribute<AgenixEndpointAttribute>();

            foreach (var attribute in field.GetCustomAttributes())
            {
                var annotationType = attribute.GetType();
                if (annotationType.IsDefined(typeof(AgenixEndpointConfigAttribute)))
                {
                    var endpoint = context.EndpointFactory.Create(GetEndpointName(field), attribute, context);
                    ReflectionHelper.SetField(field, target, endpoint);

                    if (field.IsDefined(typeof(BindToRegistryAttribute)))
                    {
                        var bindToRegistry = field.GetCustomAttribute<BindToRegistryAttribute>();
                        var referenceName = ReferenceRegistry.GetName(bindToRegistry, endpoint.Name);
                        context.ReferenceResolver.Bind(referenceName, endpoint);
                    }

                    return;
                }
            }

            var referenceResolver = context.ReferenceResolver;
            if (endpointAnnotation.Properties.Length > 0)
            {
                var endpoint = context.EndpointFactory.Create(GetEndpointName(field), endpointAnnotation,
                    field.FieldType, context);
                ReflectionHelper.SetField(field, target, endpoint);
            }
            else if (!string.IsNullOrWhiteSpace(endpointAnnotation.Name) &&
                     referenceResolver.IsResolvable(endpointAnnotation.Name))
            {
                var resolvedEndpoint = referenceResolver.Resolve<dynamic>([endpointAnnotation.Name],
                    field.FieldType);

                ReflectionHelper.SetField(field, target,
                    resolvedEndpoint is { Count: > 0 } ? resolvedEndpoint[0] : resolvedEndpoint);
            }
            else if (referenceResolver.IsResolvable(field.Name))
            {
                var resolvedEndpoint = referenceResolver.Resolve<dynamic>([field.Name], field.FieldType);
                ReflectionHelper.SetField(field, target, resolvedEndpoint);
            }
            else
            {
                var resolvedEndpoint = referenceResolver.Resolve<dynamic>();
                ReflectionHelper.SetField(field, target, resolvedEndpoint);
            }
        });
    }

    /// <summary>
    ///     Either reads AgenixEndpoint name property or constructs endpoint name from field name.
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    private static string GetEndpointName(FieldInfo field)
    {
        var attribute = field.GetCustomAttribute<AgenixEndpointAttribute>();

        if (attribute != null && !string.IsNullOrWhiteSpace(attribute.Name))
        {
            return attribute.Name;
        }

        return field.Name;
    }
}
