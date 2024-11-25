using System.Reflection;
using Agenix.Core.Endpoint;
using Agenix.Core.Spi;
using Agenix.Core.Util;
using log4net;

namespace Agenix.Core.Annotations;

/// <summary>
///     Dependency injection support for AgenixEndpoint endpoint annotations.
/// </summary>
public abstract class AgenixEndpointAnnotations
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(AgenixEndpointAnnotations));

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
                !typeof(IEndpoint).IsAssignableFrom(field.FieldType)) return;

            Log.Debug($"Injecting Agenix endpoint on test class field '{field.Name}'");
            var endpointAnnotation = field.GetCustomAttribute<AgenixEndpointAttribute>();

            foreach (var attribute in field.GetCustomAttributes())
            {
                var annotationType = attribute.GetType();
                if (annotationType.IsDefined(typeof(AgenixEndpointConfigAttribute)))
                {
                    var endpoint = context.EndpointFactory.Create(GetEndpointName(field), attribute, context);
                    ReflectionHelper.SetField(field, target, endpoint);

                    if (field.IsDefined(typeof(BindToRegistry)))
                    {
                        var bindToRegistry = field.GetCustomAttribute<BindToRegistry>();
                        var referenceName = ReferenceRegistry.GetName(bindToRegistry, endpoint.Name);
                        context.GetReferenceResolver().Bind(referenceName, endpoint);
                    }

                    return;
                }
            }

            var referenceResolver = context.GetReferenceResolver();
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
                ReflectionHelper.SetField(field, target, resolvedEndpoint);
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

        /*var fields = target.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
        foreach (var field in fields)
        {
            if (!field.IsDefined(typeof(AgenixEndpointAttribute)) ||
                !typeof(IEndpoint).IsAssignableFrom(field.FieldType)) continue;

            Log.Debug($"Injecting Agenix endpoint on test class field '{field.Name}'");
            var endpointAnnotation = field.GetCustomAttribute<AgenixEndpointAttribute>();

            foreach (var attribute in field.GetCustomAttributes())
            {
                var annotationType = attribute.GetType();
                if (annotationType.IsDefined(typeof(AgenixEndpointConfigAttribute)))
                {
                    var endpoint = context.EndpointFactory.Create(GetEndpointName(field), attribute, context);
                    ReflectionHelper.SetField(field, target, endpoint);

                    if (field.IsDefined(typeof(BindToRegistry)))
                    {
                        var bindToRegistry = field.GetCustomAttribute<BindToRegistry>();
                        var referenceName = ReferenceRegistry.GetName(bindToRegistry, endpoint.Name);
                        context.GetReferenceResolver().Bind(referenceName, endpoint);
                    }

                    return;
                }
            }

            var referenceResolver = context.GetReferenceResolver();
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
                ReflectionHelper.SetField(field, target, resolvedEndpoint);
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
            }*/
    }

    /// <summary>
    ///     Either reads CitrusEndpoint name property or constructs endpoint name from field name.
    /// </summary>
    /// <param name="field"></param>
    /// <returns></returns>
    private static string GetEndpointName(FieldInfo field)
    {
        var attribute = field.GetCustomAttribute<AgenixEndpointAttribute>();

        if (attribute != null && !string.IsNullOrWhiteSpace(attribute.Name)) return attribute.Name;

        return field.Name;
    }
}