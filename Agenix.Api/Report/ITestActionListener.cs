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

/// <summary>
///     Interface to listen for actions performed on test cases, including start, finish, and skip events.
/// </summary>
public interface ITestActionListener
{
    /// <summary>
    ///     Invoked when a test action is started.
    /// </summary>
    /// <param name="testCase">The test case that contains the started action.</param>
    /// <param name="testAction">The specific test action that was started.</param>
    void OnTestActionStart(ITestCase testCase, ITestAction testAction);

    /// <summary>
    ///     Invoked when a test action is finished.
    /// </summary>
    /// <param name="testCase">The test case that contains the finished action.</param>
    /// <param name="testAction">The specific test action that was finished.</param>
    void OnTestActionFinish(ITestCase testCase, ITestAction testAction);

    /// <summary>
    ///     Invoked when a test action is skipped.
    /// </summary>
    /// <param name="testCase">The test case that contains the skipped action.</param>
    /// <param name="testAction">The specific test action that was skipped.</param>
    void OnTestActionSkipped(ITestCase testCase, ITestAction testAction);
}
