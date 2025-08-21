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

using System.Collections.ObjectModel;

namespace Agenix.Api.Report;

/// Class managing a list of injected test suite listeners. Each event is spread to all
/// managed listeners.
/// /
public class AsyncTestSuiteListeners : IAsyncTestSuiteListenerAware
{
    /** List of testsuite listeners **/
    private readonly List<IAsyncTestSuiteListener> _testSuiteListeners = [];

    /// Provides a read-only collection of the currently registered test suite listeners.
    public ReadOnlyCollection<IAsyncTestSuiteListener> GetTestSuiteListeners => new(_testSuiteListeners);

    /// Adds a test suite listener to the collection of listeners.
    /// <param name="suiteListener">The test suite listener to be added.</param>
    public void AddTestSuiteListener(IAsyncTestSuiteListener suiteListener)
    {
        if (!_testSuiteListeners.Contains(suiteListener))
        {
            _testSuiteListeners.Add(suiteListener);
        }
    }

    /// Invoked after the test suite has finished and notifies all registered listeners by calling their OnFinish method.
    public async Task OnFinish()
    {
        foreach (var listener in _testSuiteListeners)
        {
            await listener.OnFinish();
        }
    }

    /// Invoked after the test suite has failed to finish.
    /// <param name="cause">The exception cause of the failure.</param>
    public async Task OnFinishFailure(Exception cause)
    {
        foreach (var listener in _testSuiteListeners)
        {
            await listener.OnFinishFailure(cause);
        }
    }

    /// Invoked after the test suite has successfully finished.
    public async Task OnFinishSuccess()
    {
        foreach (var listener in _testSuiteListeners)
        {
            await listener.OnFinishSuccess();
        }
    }

    /// Invoked when the test suite starts.
    public async Task OnStart()
    {
        foreach (var listener in _testSuiteListeners)
        {
            await listener.OnStart();
        }
    }

    /// Invoked after the test suite has failed to start.
    /// <param name="cause">The exception cause of the failure.</param>
    public async Task OnStartFailure(Exception cause)
    {
        foreach (var listener in _testSuiteListeners)
        {
            await listener.OnStartFailure(cause);
        }
    }

    /// Invoked after the test suite has successfully started.
    public async Task OnStartSuccess()
    {
        foreach (var listener in _testSuiteListeners)
        {
            await listener.OnStartSuccess();
        }
    }
}
