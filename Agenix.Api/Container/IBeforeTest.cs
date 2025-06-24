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

namespace Agenix.Api.Container;

/// <summary>
///     Represents an interface that defines a set of actions to be performed before test execution.
/// </summary>
public interface IBeforeTest : ITestActionContainer
{
    /// <summary>
    ///     Checks if this suite action should execute, according to suite name and included test groups.
    /// </summary>
    /// <param name="testName">The name of the test.</param>
    /// <param name="packageName">The name of the package.</param>
    /// <param name="includedGroups">The groups of tests included.</param>
    /// <returns>Returns true if the actions should execute; otherwise false.</returns>
    bool ShouldExecute(string testName, string packageName, string[] includedGroups);
}
