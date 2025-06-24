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

namespace Agenix.Api.Report;

/// Class responsible for spreading test events to all available test listeners
/// injected by Spring's IoC container.
/// /
public class TestListeners : ITestListenerAware
{
    /** List of test listeners **/
    private readonly List<ITestListener> _testListeners = [];

    /// Adds a test listener to the list of test listeners.
    /// <param name="listener">The test listener to add.</param>
    public void AddTestListener(ITestListener listener)
    {
        if (!_testListeners.Contains(listener))
        {
            _testListeners.Add(listener);
        }
    }

    /// Notifies all test listeners about the failure of a test.
    /// <param name="test">The test case that has failed.</param>
    /// <param name="cause">The exception that caused the failure.</param>
    public void OnTestFailure(ITestCase test, Exception cause)
    {
        foreach (var listener in _testListeners)
        {
            listener.OnTestFailure(test, cause);
        }
    }

    /// Notifies all test listeners about the completion of a test.
    /// <param name="test">The test case that has finished.</param>
    public void OnTestFinish(ITestCase test)
    {
        foreach (var listener in _testListeners)
        {
            listener.OnTestFinish(test);
        }
    }

    /// Notifies all test listeners about a skipped test.
    /// <param name="test">The test case that is skipped.</param>
    public void OnTestSkipped(ITestCase test)
    {
        foreach (var listener in _testListeners)
        {
            listener.OnTestSkipped(test);
        }
    }

    /// Notifies all test listeners about the start of a test.
    /// <param name="test">The test case that is starting.</param>
    public void OnTestStart(ITestCase test)
    {
        foreach (var listener in _testListeners)
        {
            listener.OnTestStart(test);
        }
    }

    /// Notifies all test listeners about the successful completion of a test.
    /// <param name="test">The test case that completed successfully.</param>
    public void OnTestSuccess(ITestCase test)
    {
        foreach (var listener in _testListeners)
        {
            listener.OnTestSuccess(test);
        }
    }

    /// Obtains the testListeners.
    /// @return A list of test listeners.
    /// /
    public List<ITestListener> GetTestListeners()
    {
        return _testListeners;
    }
}
