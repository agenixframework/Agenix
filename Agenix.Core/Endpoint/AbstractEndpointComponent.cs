using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Agenix.Api.Context;
using Agenix.Api.Endpoint;
using Agenix.Api.Exceptions;
using Agenix.Api.Spi;
using Agenix.Api.Util;

namespace Agenix.Core.Endpoint;

public abstract class AbstractEndpointComponent(string name) : IEndpointComponent
{
    /// <summary>
    ///     Creates an endpoint using the specified URI and context.
    /// </summary>
    /// <param name="endpointUri">The URI of the endpoint.</param>
    /// <param name="context">The context in which the endpoint is created.</param>
    /// <returns>The created endpoint.</returns>
    /// <exception cref="AgenixSystemException">Thrown when the endpoint URI is invalid.</exception>
    public IEndpoint CreateEndpoint(string endpointUri, TestContext context)
    {
        try
        {
            var uri = new Uri(endpointUri);
            string path;

            if (endpointUri.StartsWith("http://") || endpointUri.StartsWith("https://"))
                path = uri.GetComponents(UriComponents.HostAndPort, UriFormat.Unescaped) +
                       uri.GetComponents(UriComponents.PathAndQuery, UriFormat.Unescaped);
            else
                path = uri.GetComponents(UriComponents.PathAndQuery, UriFormat.Unescaped);

            if (path.StartsWith("//")) path = path[2..];

            if (path.Contains('?')) path = path[..path.IndexOf('?')];

            var parameters = GetParameters(uri.ToString());
            string endpointName = null;
            if (parameters.TryGetValue(IEndpointComponent.EndpointName, out var value))
            {
                endpointName = value;
                parameters.Remove(IEndpointComponent.EndpointName);
            }

            var endpoint = CreateEndpoint(path, parameters, context);

            var referenceResolverAware = endpoint as IReferenceResolverAware;
            if (referenceResolverAware != null)
                referenceResolverAware.SetReferenceResolver(context.ReferenceResolver);

            if (!string.IsNullOrEmpty(endpointName)) endpoint.SetName(endpointName);

            return endpoint;
        }
        catch (UriFormatException ex)
        {
            throw new AgenixSystemException($"Unable to parse endpoint URI '{endpointUri}'", ex);
        }
    }

    /// <summary>
    ///     Gets the name of this endpoint component.
    /// </summary>
    /// <returns>The name of the endpoint component.</returns>
    public string GetName()
    {
        return name;
    }

    /// <summary>
    ///     Parses the given endpoint URI and extracts the parameters from the query string.
    /// </summary>
    /// <param name="endpointUri">The endpoint URI to extract parameters from.</param>
    /// <returns>A dictionary containing the parameters as key-value pairs.</returns>
    public IDictionary<string, string> GetParameters(string endpointUri)
    {
        var parameters = new Dictionary<string, string>();

        if (endpointUri.Contains('?'))
        {
            var parameterString = endpointUri[(endpointUri.IndexOf('?') + 1)..];

            var tokens = parameterString.Split('&');
            foreach (var token in tokens)
            {
                var parameterValue = token.Split('=');
                if (parameterValue.Length == 1)
                    parameters[parameterValue[0]] = null;
                else if (parameterValue.Length == 2)
                    parameters[parameterValue[0]] = parameterValue[1];
                else
                    throw new AgenixSystemException(
                        $"Invalid parameter key/value combination '{string.Join(", ", parameterValue)}'");
            }
        }

        return parameters;
    }

    /// <summary>
    ///     Enriches the endpoint configuration by setting its fields using the provided parameters and context.
    /// </summary>
    /// <param name="endpointConfiguration">The endpoint configuration to be enriched.</param>
    /// <param name="parameters">
    ///     A dictionary containing the parameter names and values to be set on the endpoint
    ///     configuration.
    /// </param>
    /// <param name="context">The test context providing additional context information for parameter conversion.</param>
    protected void EnrichEndpointConfiguration(IEndpointConfiguration endpointConfiguration,
        IDictionary<string, string> parameters, TestContext context)
    {
        foreach (var parameterEntry in parameters)
        {
            // Find the field in the endpoint configuration type using reflection
            var field = ReflectionHelper.FindField(endpointConfiguration.GetType(), parameterEntry.Key);

            if (field == null)
                throw new AgenixSystemException(
                    $"Unable to find parameter field on endpoint configuration '{parameterEntry.Key}'");

            // Find the corresponding setter method
            var setterName = "Set" + char.ToUpper(parameterEntry.Key[0]) + parameterEntry.Key[1..];
            var setter = ReflectionHelper.FindMethod(endpointConfiguration.GetType(), setterName, field.FieldType);

            if (setter == null)
            {
                setterName = char.ToUpper(parameterEntry.Key[0]) + parameterEntry.Key[1..];
                setter = ReflectionHelper.FindMethod(endpointConfiguration.GetType(), setterName, field.FieldType);
            }

            if (setter == null)
                throw new AgenixSystemException(
                    $"Unable to find parameter setter on endpoint configuration '{setterName}'");

            // Convert the parameter value to the appropriate type and invoke the setter method
            if (parameterEntry.Value != null)
            {
                var convertedValue =
                    TypeConversionUtils.ConvertStringToType<dynamic>(parameterEntry.Value, field.FieldType, context);
                ReflectionHelper.InvokeMethod(setter, endpointConfiguration, convertedValue);
            }
            else
            {
                ReflectionHelper.InvokeMethod(setter, endpointConfiguration, null);
            }
        }
    }

    /// <summary>
    ///     Retrieves the configuration parameters for the specified endpoint type.
    /// </summary>
    /// <param name="parameters">The initial set of parameters.</param>
    /// <param name="endpointConfigurationType">The type of the endpoint configuration.</param>
    /// <returns>A dictionary containing the filtered configuration parameters for the endpoint type.</returns>
    protected IDictionary<string, string> GetEndpointConfigurationParameters(
        IDictionary<string, string> parameters,
        Type endpointConfigurationType)
    {
        return (from parameterEntry in parameters
                let field = ReflectionHelper.FindField(endpointConfigurationType, parameterEntry.Key)
                where field != null
                select parameterEntry)
            .ToDictionary(parameterEntry => parameterEntry.Key, parameterEntry => parameterEntry.Value);
    }

    /// <summary>
    ///     Converts a dictionary of parameters into a URL parameter string.
    /// </summary>
    /// <param name="parameters">The dictionary of parameters to convert.</param>
    /// <param name="endpointConfigurationType">The type used to reflect and find fields.</param>
    /// <returns>A string representation of the URL parameters.</returns>
    protected string GetParameterString(IDictionary<string, string> parameters, Type endpointConfigurationType)
    {
        var paramString = new StringBuilder();

        foreach (var parameterEntry in parameters)
        {
            // Use reflection to find the field in the given type
            var field = ReflectionHelper.FindField(endpointConfigurationType, parameterEntry.Key);

            if (field != null) continue;
            if (paramString.Length == 0)
                paramString.Append('?').Append(parameterEntry.Key);
            else
                paramString.Append('&').Append(parameterEntry.Key);

            if (parameterEntry.Value != null) paramString.Append('=').Append(parameterEntry.Value);
        }

        return paramString.ToString();
    }

    /// <summary>
    ///     Creates an endpoint with the specified URI and context.
    /// </summary>
    /// <param name="endpointUri">The URI of the endpoint to create.</param>
    /// <param name="context">The context in which the endpoint is being created.</param>
    /// <returns>A new instance of an endpoint.</returns>
    protected abstract IEndpoint CreateEndpoint(string resourcePath, IDictionary<string, string> parameters,
        TestContext context);
}
