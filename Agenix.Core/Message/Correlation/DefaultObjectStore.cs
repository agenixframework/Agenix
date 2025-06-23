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

using System.Collections.Concurrent;
using Agenix.Api.Message.Correlation;

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
