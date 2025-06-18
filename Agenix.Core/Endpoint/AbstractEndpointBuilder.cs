using System;
using Agenix.Api.Common;
using Agenix.Api.Endpoint;
using Agenix.Api.Exceptions;

namespace Agenix.Core.Endpoint;

public abstract class AbstractEndpointBuilder<T> : IEndpointBuilder<T> where T : IEndpoint
{
    public T Build()
    {
        return GetEndpoint();
    }

    public virtual bool Supports(Type endpointType)
    {
        return GetEndpoint().GetType() == endpointType;
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
    /// <exception cref="AgenixSystemException"></exception>
    public AbstractEndpointBuilder<T> Initialize()
    {
        if (GetEndpoint() is InitializingPhase phase)
        {
            try
            {
                phase.Initialize();
            }
            catch (Exception e)
            {
                throw new AgenixSystemException("Failed to initialize server", e);
            }
        }

        return this;
    }

    /// <summary>
    ///     Gets the target endpoint instance.
    /// </summary>
    /// <returns></returns>
    protected abstract T GetEndpoint();
}
