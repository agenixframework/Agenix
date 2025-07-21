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

#endregion

namespace Agenix.Playwright.Endpoint;

/// <summary>
///     Defines the test isolation strategy for Playwright browser tests.
/// </summary>
public enum TestIsolationMode
{
    /// <summary>
    ///     No test isolation - all tests share the same context and page.
    /// </summary>
    NONE,

    /// <summary>
    ///     Create a new browser context for each test (recommended).
    ///     Provides complete isolation, including cookies, local storage, and session data.
    /// </summary>
    NEW_CONTEXT_PER_TEST,

    /// <summary>
    ///     Create a new page in the same context for each test.
    ///     Provides basic isolation but shares cookies and session data.
    /// </summary>
    NEW_PAGE_PER_TEST
}
