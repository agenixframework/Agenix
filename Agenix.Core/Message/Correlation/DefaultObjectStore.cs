using System.Collections.Concurrent;

namespace Agenix.Core.Message.Correlation;

/// <summary>
///     Default object store implementation works on simple in memory dictionary map.
/// </summary>
/// <typeparam name="T"></typeparam>
public class DefaultObjectStore<T> : IObjectStore<T>
{
    private readonly ConcurrentDictionary<string, T> _store = new();

    /// <summary>
    ///     Adds a new object with the correlation key.
    /// </summary>
    /// <param name="correlationKey">The correlation key.</param>
    /// <param name="obj">The object to add.</param>
    public void Add(string correlationKey, T obj)
    {
        _store[correlationKey] = obj;
    }

    /// <summary>
    ///     Removes the object with the correlation key.
    /// </summary>
    /// <param name="correlationKey">The correlation key.</param>
    /// <returns>The removed object.</returns>
    public T Remove(string correlationKey)
    {
        _store.TryRemove(correlationKey, out var removedObject);
        return removedObject;
    }
}