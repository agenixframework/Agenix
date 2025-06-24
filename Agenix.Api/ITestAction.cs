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
///     The test context provides utility methods for replacing dynamic content(variables and functions) in string
/// </summary>
public interface ITestAction
{
    /// <summary>
    ///     Name of test action injected as Spring bean name
    /// </summary>
    /// <returns>name as String</returns>
    string Name => GetType().Name;

    /// <summary>
    ///     Main execution method doing all work
    /// </summary>
    void Execute(TestContext context);

    /// <summary>
    ///     Checks if this action is disabled.
    /// </summary>
    /// <param name="context">the current test context.</param>
    /// <returns>true if action is marked disabled.</returns>
    bool IsDisabled(TestContext context)
    {
        return false;
    }
}

/// <summary>
///     Delegate for creating an instance of an ITestAction based on a given TestContext.
/// </summary>
public delegate void TestAction(TestContext context);
