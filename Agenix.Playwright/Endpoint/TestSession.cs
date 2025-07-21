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
///     Represents a test session with its associated context and pages.
///     Used for tracking and managing test isolation resources.
/// </summary>
/// <param name="TestId">Unique identifier for the test</param>
/// <param name="ContextId">Browser context ID for this test session</param>
/// <param name="PageIds">List of page IDs associated with this test session</param>
/// <param name="StartTime">When the test session was started</param>
public record TestSession(
    string TestId,
    string ContextId,
    List<string> PageIds,
    DateTime StartTime)
{
    /// <summary>
    ///     Gets the duration of the test session.
    /// </summary>
    public TimeSpan Duration => DateTime.UtcNow - StartTime;

    /// <summary>
    ///     Gets the number of pages in this session.
    /// </summary>
    public int PageCount => PageIds.Count;

    /// <summary>
    ///     Gets the primary page ID for this session.
    /// </summary>
    public string? PrimaryPageId => PageIds.FirstOrDefault();
}
