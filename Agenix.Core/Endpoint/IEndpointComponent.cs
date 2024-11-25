using System.Collections.Generic;
using Agenix.Core.Endpoint.Direct;
using Agenix.Core.Exceptions;
using Agenix.Core.Util;
using log4net;

namespace Agenix.Core.Endpoint;

/// <summary>
///     Endpoint component registers with bean name in Spring application context and is then responsible to create proper
///     endpoints dynamically from endpoint uri values. Creates endpoint instance by parsing the dynamic endpoint uri with
///     special properties and parameters. Creates proper endpoint configuration instance on the fly.
/// </summary>
public interface IEndpointComponent
{
    public static string EndpointName = "endpointName";


    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(IEndpointComponent));

    /// <summary>
    ///     Creates an endpoint instance by parsing the given dynamic endpoint URI with specific properties and parameters.
    /// </summary>
    /// <param name="endpointUri">The URI of the endpoint to be created.</param>
    /// <param name="context">The context in which the endpoint is to be created.</param>
    /// <returns>A new instance of an IEndpoint configured according to the specified URI and context.</returns>
    IEndpoint CreateEndpoint(string endpointUri, TestContext context);

    /// <summary>
    ///     Gets the name of this endpoint component.
    /// </summary>
    /// <returns>The name of the endpoint component.</returns>
    string GetName();

    /// <summary>
    ///     Constructs endpoint parameters from the endpoint URI.
    /// </summary>
    /// <param name="endpointUri">The endpoint URI.</param>
    /// <returns>A dictionary of parameters extracted from the endpoint URI.</returns>
    IDictionary<string, string> GetParameters(string endpointUri);

    /// <summary>
    ///     Provides a lookup of available endpoint components, indexed by their respective types.
    /// </summary>
    /// <returns>
    ///     A dictionary where the key is a string representing the endpoint type and the value is an instance of
    ///     IEndpointComponent.
    /// </returns>
    public static Dictionary<string, IEndpointComponent> Lookup()
    {
        var validators = new Dictionary<string, IEndpointComponent>
        {
            { "direct", new DirectEndpointComponent() }
        };

        if (!Log.IsDebugEnabled) return validators;
        foreach (var kvp in validators)
            Log.Debug($"Found annotation config parser '{kvp.Key}' as {kvp.Value.GetType().Name}");

        return validators;
    }

    /// <summary>
    ///     Retrieves an <see cref="IEndpointComponent" /> instance based on the specified validator string.
    ///     If the validator is recognized, a corresponding <see cref="IEndpointComponent" /> instance is instantiated and
    ///     returned.
    ///     Otherwise, an empty <see cref="Optional{T}" /> is returned.
    /// </summary>
    /// <param name="validator">The validator string used to lookup the appropriate <see cref="IEndpointComponent" /> instance.</param>
    /// <returns>
    ///     An <see cref="Optional{T}" /> containing the <see cref="IEndpointComponent" /> instance if found, otherwise an
    ///     empty <see cref="Optional{T}" />.
    /// </returns>
    public static Optional<IEndpointComponent> Lookup(string validator)
    {
        try
        {
            switch (validator)
            {
                case "direct":
                {
                    var instance = new DirectEndpointComponent();
                    return Optional<IEndpointComponent>.Of(instance);
                }
            }
        }
        catch (CoreSystemException)
        {
            Log.Warn($"Failed to resolve annotation config parser from resource '{validator}'");
        }

        return Optional<IEndpointComponent>.Empty;
    }
}