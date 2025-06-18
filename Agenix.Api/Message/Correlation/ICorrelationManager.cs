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

using Agenix.Api.Context;

namespace Agenix.Api.Message.Correlation;

/// <summary>
///     Correlation manager stores objects with a correlation key. Clients can access the same objects some time later with
///     the same correlation key. This mechanism is used in synchronous communication where request and response messages
///     are
///     stored for correlating consumer and producer components.
/// </summary>
/// <typeparam name="T"></typeparam>
public interface ICorrelationManager<T>
{
    /// <summary>
    ///     Creates a new correlation key in the test context by saving it as a test variable.
    ///     Method is called when synchronous communication is initialized.
    /// </summary>
    /// <param name="correlationKeyName">The name of the correlation key variable.</param>
    /// <param name="correlationKey">The correlation key value.</param>
    /// <param name="context">The test context.</param>
    void SaveCorrelationKey(string correlationKeyName, string correlationKey, TestContext context);

    /// <summary>
    ///     Gets the correlation key for the given identifier.
    ///     Consults the test context with test variables for retrieving the stored correlation key.
    /// </summary>
    /// <param name="correlationKeyName">The name of the correlation key variable.</param>
    /// <param name="context">The test context.</param>
    /// <returns>The correlation key.</returns>
    string GetCorrelationKey(string correlationKeyName, TestContext context);

    /// <summary>
    ///     Stores an object in the correlation storage using the given correlation key.
    /// </summary>
    /// <param name="correlationKey">The correlation key.</param>
    /// <param name="obj">The object to store.</param>
    void Store(string correlationKey, T obj);

    /// <summary>
    ///     Finds the stored object by its correlation key.
    /// </summary>
    /// <param name="correlationKey">The correlation key.</param>
    /// <param name="timeout">The timeout period in milliseconds.</param>
    /// <returns>The found object.</returns>
    T Find(string correlationKey, long timeout);

    /// <summary>
    ///     Sets the object store implementation.
    /// </summary>
    /// <param name="store">The object store.</param>
    void SetObjectStore(IObjectStore<T> store);

    /// <summary>
    ///     Gets the object store implementation.
    /// </summary>
    /// <returns>The object store.</returns>
    IObjectStore<T> GetObjectStore();
}
