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

using System;
using Agenix.Api;

namespace Agenix.Core;

/// <summary>
///     Provides methods to create instances of TestResult for different outcomes of a test case.
///     Implementors may decide to create TestResults with specific parameters derived from the TestCase.
/// </summary>
public interface ITestResultInstanceProvider
{
    /// <summary>
    ///     Creates a successful <see cref="TestResult" /> instance for the given test case.
    /// </summary>
    /// <param name="testCase">The test case for which the success result should be created.</param>
    /// <returns>A <see cref="TestResult" /> instance indicating the test case has succeeded.</returns>
    TestResult CreateSuccess(ITestCase testCase);

    /// <summary>
    ///     Creates a failed <see cref="TestResult" /> instance for the given test case.
    /// </summary>
    /// <param name="testCase">The test case for which the failure result should be created.</param>
    /// <param name="ex">The exception that caused the test case to fail.</param>
    /// <returns>A <see cref="TestResult" /> instance indicating the test case has failed.</returns>
    TestResult CreateFailed(ITestCase testCase, Exception ex);

    /// <summary>
    ///     Creates a skipped <see cref="TestResult" /> instance for the given test case.
    /// </summary>
    /// <param name="testCase">The test case for which the skipped result should be created.</param>
    /// <returns>A <see cref="TestResult" /> instance indicating the test case has been skipped.</returns>
    TestResult CreateSkipped(ITestCase testCase);
}
