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
public class TestSuiteListeners : ITestSuiteListenerAware
{
    /** List of testsuite listeners **/
    private readonly List<ITestSuiteListener> _testSuiteListeners = [];

    /// Provides a read-only collection of the currently registered test suite listeners.
    public ReadOnlyCollection<ITestSuiteListener> GetTestSuiteListeners => new(_testSuiteListeners);

    /// Adds a test suite listener to the collection of listeners.
    /// <param name="testSuiteListener">The test suite listener to be added.</param>
    public void AddTestSuiteListener(ITestSuiteListener testSuiteListener)
    {
        if (!_testSuiteListeners.Contains(testSuiteListener))
        {
            _testSuiteListeners.Add(testSuiteListener);
        }
    }

    /// Invoked after the test suite has finished.
    public void OnFinish()
    {
        foreach (var listener in _testSuiteListeners)
        {
            listener.OnFinish();
        }
    }

    /// Invoked after the test suite has failed to finish.
    /// <param name="cause">The exception cause of the failure.</param>
    public void OnFinishFailure(Exception cause)
    {
        foreach (var listener in _testSuiteListeners)
        {
            listener.OnFinishFailure(cause);
        }
    }

    /// Invoked after the test suite has successfully finished.
    public void OnFinishSuccess()
    {
        foreach (var listener in _testSuiteListeners)
        {
            listener.OnFinishSuccess();
        }
    }

    /// Invoked when the test suite starts.
    public void OnStart()
    {
        foreach (var listener in _testSuiteListeners)
        {
            listener.OnStart();
        }
    }

    /// Invoked after the test suite has failed to start.
    /// <param name="cause">The exception cause of the failure.</param>
    public void OnStartFailure(Exception cause)
    {
        foreach (var listener in _testSuiteListeners)
        {
            listener.OnStartFailure(cause);
        }
    }

    /// Invoked after the test suite has successfully started.
    public void OnStartSuccess()
    {
        foreach (var listener in _testSuiteListeners)
        {
            listener.OnStartSuccess();
        }
    }
}
