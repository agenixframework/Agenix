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

/// Interface representing a test listener. Invoked at various stages of the test lifecycle.
/// /
public interface ITestListener
{
    /// Invoked when test gets started
    /// @param test Test case to be started
    /// /
    void OnTestStart(ITestCase test);

    /// Invoked when test gets finished
    /// @param test Test case that has finished
    void OnTestFinish(ITestCase test);

    /// Invoked when test finished with success
    /// <param name="test">Test case that finished successfully</param>
    void OnTestSuccess(ITestCase test);

    /// Invoked when test finished with failure
    /// <param name="test">Test case that finished with failure</param>
    /// <param name="cause">Exception that caused the test failure</param>
    void OnTestFailure(ITestCase test, Exception cause);

    /// Invoked when test is skipped
    /// <param name="test">Test case that is being skipped</param>
    void OnTestSkipped(ITestCase test);
}
