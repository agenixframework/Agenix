using System;
using Agenix.Api.Context;
using Agenix.Api.Message.Correlation;
using log4net;

namespace Agenix.Core.Message.Correlation;

/// <summary>
///     Default correlation manager implementation works on simple in memory map for storing objects. Correlation key is
///     the map key. Clients can access objects in the store using the correlation key.
/// </summary>
/// <typeparam name="T"></typeparam>
public class DefaultCorrelationManager<T> : ICorrelationManager<T>
{
    private static readonly ILog Log = LogManager.GetLogger("DefaultCorrelationManager");

    private IObjectStore<T> _objectStore = new DefaultObjectStore<T>();

    /// <summary>
    ///     Saves the specified correlation key into the given context under the provided correlation key name.
    /// </summary>
    /// <param name="correlationKeyName">The name used to save the correlation key.</param>
    /// <param name="correlationKey">The correlation key to be saved.</param>
    /// <param name="context">The context in which the correlation key will be saved.</param>
    public void SaveCorrelationKey(string correlationKeyName, string correlationKey, TestContext context)
    {
        if (Log.IsDebugEnabled) Log.Debug($"Saving correlation key for '{correlationKeyName}'");

        context.SetVariable(correlationKeyName, correlationKey);
    }

    /// <summary>
    ///     Retrieves the correlation key associated with the specified correlation key name from the given context.
    /// </summary>
    /// <param name="correlationKeyName">The name of the correlation key to retrieve.</param>
    /// <param name="context">The context containing the correlation key.</param>
    /// <returns>The correlation key associated with the specified name.</returns>
    /// <exception cref="Exception">Thrown if the correlation key could not be found in the context.</exception>
    public virtual string GetCorrelationKey(string correlationKeyName, TestContext context)
    {
        if (Log.IsDebugEnabled) Log.Debug($"Get correlation key for '{correlationKeyName}'");

        if (context.GetVariables().ContainsKey(correlationKeyName)) return context.GetVariable(correlationKeyName);

        throw new Exception($"Failed to get correlation key for '{correlationKeyName}'");
    }

    /// <summary>
    ///     Stores the specified object associated with the given correlation key.
    /// </summary>
    /// <param name="correlationKey">The correlation key associated with the object.</param>
    /// <param name="obj">The object to be stored.</param>
    public void Store(string correlationKey, T obj)
    {
        if (obj == null)
        {
            Log.Warn($"Ignore correlated null object for '{correlationKey}'");
            return;
        }

        if (Log.IsDebugEnabled) Log.Debug($"Saving correlated object for '{correlationKey}'");

        _objectStore.Add(correlationKey, obj);
    }

    /// <summary>
    ///     Finds and removes the object associated with the specified correlation key.
    /// </summary>
    /// <param name="correlationKey">The correlation key associated with the object.</param>
    /// <param name="timeout">The maximum amount of time to wait for the object to be found, in milliseconds.</param>
    /// <returns>The object associated with the specified correlation key, or the default value if not found.</returns>
    public virtual T Find(string correlationKey, long timeout)
    {
        if (Log.IsDebugEnabled) Log.Debug($"Finding correlated object for '{correlationKey}'");

        return _objectStore.Remove(correlationKey);
    }

    /// <summary>
    ///     Sets the object store to be used by the correlation manager.
    /// </summary>
    /// <param name="store">The object store to be set.</param>
    public void SetObjectStore(IObjectStore<T> store)
    {
        _objectStore = store;
    }

    /// <summary>
    ///     Retrieves the current object store used by the correlation manager.
    /// </summary>
    /// <returns>The object store currently in use.</returns>
    public IObjectStore<T> GetObjectStore()
    {
        return _objectStore;
    }
}