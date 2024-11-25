namespace Agenix.Core.Message.Correlation;

/// <summary>
///     A generic interface for storing and managing objects with correlation keys.
/// </summary>
/// <typeparam name="T">The type of objects to be stored.</typeparam>
public interface IObjectStore<T>
{
    /// <summary>
    ///     Adds a new object with a correlation key.
    /// </summary>
    /// <param name="correlationKey">The correlation key associated with the object.</param>
    /// <param name="obj">The object to be added.</param>
    void Add(string correlationKey, T obj);

    /// <summary>
    ///     Removes the object with the specified correlation key.
    /// </summary>
    /// <param name="correlationKey">The correlation key associated with the object.</param>
    /// <returns>The removed object.</returns>
    T Remove(string correlationKey);
}