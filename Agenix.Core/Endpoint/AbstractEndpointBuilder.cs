using System;
using Agenix.Core.Common;
using Agenix.Core.Exceptions;

namespace Agenix.Core.Endpoint;

public abstract class AbstractEndpointBuilder<T> : IEndpointBuilder<T> where T : IEndpoint
{
    public T Build()
    {
        return GetEndpoint();
    }

    public bool Supports(Type endpointType)
    {
        return GetEndpoint().GetType().Equals(endpointType);
    }

    /// <summary>
    ///     Sets the endpoint name.
    /// </summary>
    /// <param name="endpointName">the endpoint name</param>
    /// <returns></returns>
    public AbstractEndpointBuilder<T> Name(string endpointName)
    {
        GetEndpoint().SetName(endpointName);
        return this;
    }

    /// <summary>
    ///     Initializes the endpoint.
    /// </summary>
    /// <returns></returns>
    /// <exception cref="CoreSystemException"></exception>
    public AbstractEndpointBuilder<T> Initialize()
    {
        var phase = GetEndpoint() as InitializingPhase;
        if (phase != null)
            try
            {
                phase.Initialize();
            }
            catch (Exception e)
            {
                throw new CoreSystemException("Failed to initialize server", e);
            }

        return this;
    }

    /// <summary>
    ///     Gets the target endpoint instance.
    /// </summary>
    /// <returns></returns>
    protected abstract T GetEndpoint();
}