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

using Agenix.Api.Context;

namespace Agenix.Api;

/// <summary>
///     Interface for providing TestCaseRunner.
/// </summary>
public interface ITestCaseRunnerProvider
{
    /// Creates a TestCaseRunner which runs the given TestContext.
    /// @param context Test execution context.
    /// @return A new instance of ITestCaseRunner to run tests with the provided context.
    /// /
    ITestCaseRunner CreateTestCaseRunner(ITestCase testCase, TestContext context);

    /// Creates a TestCaseRunner which runs the given {@link TestCase} and the given {@link TestContext}.
    /// @param testCase The test case to be run by the TestCaseRunner.
    /// @param context The context in which the test case will be run.
    /// @return an instance of ITestCaseRunner that runs the specified test case within the given context.
    /// /
    ITestCaseRunner CreateTestCaseRunner(TestContext context);
}
