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
