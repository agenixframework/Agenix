#region License

// Licensed to the Apache Software Foundation (ASF) under one
// or more contributor license agreements. See the NOTICE file
// distributed with this work for additional information
// regarding copyright ownership. The ASF licenses this file
// to you under the Apache License, Version 2.0 (the
// "License"); you may not use this file except in compliance
// with the License. You may obtain a copy of the License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing,
// software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
// KIND, either express or implied. See the License for the
// specific language governing permissions and limitations
// under the License.
//
// Copyright (c) 2025 Agenix
//
// This file has been modified from its original form.
// Original work Copyright (C) 2006-2025 the original author or authors.

#endregion

using System.Collections.Concurrent;

namespace Agenix.Api.Report;

/// <summary>
///     Class responsible for spreading test events to all available test listeners
///     injected by Spring's IoC container.
///     This class is thread-safe and supports concurrent operations.
/// </summary>
public class TestListeners : ITestListenerAware
{
    /** Thread-safe collection of test listeners **/
    private readonly ConcurrentDictionary<ITestListener, byte> _testListeners = new();

    /// <summary>
    ///     Gets the count of currently registered test listeners.
    ///     This operation is thread-safe.
    /// </summary>
    /// <returns>The number of registered test listeners.</returns>
    public int Count => _testListeners.Count;

    /// <summary>
    ///     Adds a test listener to the collection of test listeners.
    ///     This operation is thread-safe.
    /// </summary>
    /// <param name="testListener">The test listener to add.</param>
    public void AddTestListener(ITestListener testListener)
    {
        _testListeners.TryAdd(testListener, 0);
    }

    /// <summary>
    ///     Removes a test listener from the collection of test listeners.
    ///     This operation is thread-safe.
    /// </summary>
    /// <param name="listener">The test listener to remove.</param>
    /// <returns>True if the listener was removed; false if it was not found.</returns>
    public bool RemoveTestListener(ITestListener listener)
    {
        return _testListeners.TryRemove(listener, out _);
    }

    /// <summary>
    ///     Removes all test listeners of the specified type.
    ///     This operation is thread-safe.
    /// </summary>
    /// <typeparam name="T">The type of test listeners to remove.</typeparam>
    /// <returns>The number of listeners removed.</returns>
    public int RemoveTestListenersOfType<T>() where T : ITestListener
    {
        var listenersToRemove = _testListeners.Keys.OfType<T>().ToList();

        return listenersToRemove.Count(listener => _testListeners.TryRemove(listener, out _));
    }

    /// <summary>
    ///     Clears all test listeners from the collection.
    ///     This operation is thread-safe.
    /// </summary>
    public void ClearAllTestListeners()
    {
        _testListeners.Clear();
    }

    /// <summary>
    ///     Gets all test listeners of the specified type.
    ///     This operation is thread-safe and returns a snapshot of the current listeners.
    /// </summary>
    /// <typeparam name="T">The type of test listeners to retrieve.</typeparam>
    /// <returns>A list of test listeners of the specified type.</returns>
    public List<T> GetTestListenersOfType<T>() where T : ITestListener
    {
        return _testListeners.Keys.OfType<T>().ToList();
    }

    /// <summary>
    ///     Notifies all test listeners about the failure of a test.
    ///     This operation is thread-safe.
    /// </summary>
    /// <param name="test">The test case that has failed.</param>
    /// <param name="cause">The exception that caused the failure.</param>
    public void OnTestFailure(ITestCase test, Exception cause)
    {
        var listeners = _testListeners.Keys.ToList(); // Snapshot for thread safety
        foreach (var listener in listeners)
        {
            try
            {
                listener.OnTestFailure(test, cause);
            }
            catch (Exception ex)
            {
                // Log or handle listener exceptions to prevent one failing listener from affecting others
                // Consider adding logging here based on your logging framework
            }
        }
    }

    /// <summary>
    ///     Notifies all test listeners about the completion of a test.
    ///     This operation is thread-safe.
    /// </summary>
    /// <param name="test">The test case that has finished.</param>
    public void OnTestFinish(ITestCase test)
    {
        var listeners = _testListeners.Keys.ToList(); // Snapshot for thread safety
        foreach (var listener in listeners)
        {
            try
            {
                listener.OnTestFinish(test);
            }
            catch (Exception ex)
            {
                // Log or handle listener exceptions
            }
        }
    }

    /// <summary>
    ///     Notifies all test listeners about a skipped test.
    ///     This operation is thread-safe.
    /// </summary>
    /// <param name="test">The test case that is skipped.</param>
    public void OnTestSkipped(ITestCase test)
    {
        var listeners = _testListeners.Keys.ToList(); // Snapshot for thread safety
        foreach (var listener in listeners)
        {
            try
            {
                listener.OnTestSkipped(test);
            }
            catch (Exception ex)
            {
                // Log or handle listener exceptions
            }
        }
    }

    /// <summary>
    ///     Notifies all test listeners about the start of a test.
    ///     This operation is thread-safe.
    /// </summary>
    /// <param name="test">The test case that is starting.</param>
    public void OnTestStart(ITestCase test)
    {
        var listeners = _testListeners.Keys.ToList(); // Snapshot for thread safety
        foreach (var listener in listeners)
        {
            try
            {
                listener.OnTestStart(test);
            }
            catch (Exception ex)
            {
                // Log or handle listener exceptions
            }
        }
    }

    /// <summary>
    ///     Notifies all test listeners about the successful completion of a test.
    ///     This operation is thread-safe.
    /// </summary>
    /// <param name="test">The test case that completed successfully.</param>
    public void OnTestSuccess(ITestCase test)
    {
        var listeners = _testListeners.Keys.ToList(); // Snapshot for thread safety
        foreach (var listener in listeners)
        {
            try
            {
                listener.OnTestSuccess(test);
            }
            catch (Exception ex)
            {
                // Log or handle listener exceptions
            }
        }
    }

    /// <summary>
    ///     Obtains all test listeners.
    ///     Returns a snapshot of the current listeners for thread safety.
    /// </summary>
    /// <returns>A list of all test listeners.</returns>
    public List<ITestListener> GetTestListeners()
    {
        return _testListeners.Keys.ToList();
    }

    /// <summary>
    ///     Checks if a specific test listener is registered.
    ///     This operation is thread-safe.
    /// </summary>
    /// <param name="listener">The test listener to check for.</param>
    /// <returns>True if the listener is registered; otherwise, false.</returns>
    public bool ContainsTestListener(ITestListener listener)
    {
        return _testListeners.ContainsKey(listener);
    }
}
