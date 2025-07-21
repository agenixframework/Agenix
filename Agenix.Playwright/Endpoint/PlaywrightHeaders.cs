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

namespace Agenix.Playwright.Endpoint;

/// <summary>
///     Constants for Playwright-related context variables and headers
/// </summary>
public static class PlaywrightHeaders
{
    /// <summary>
    ///     Special header prefix for Playwright rel acted actions
    /// </summary>
    public const string PlaywrightPrefix = "playwright_";

    /// <summary>
    ///     Context variable key for the Playwright browser instance
    /// </summary>
    public const string PlaywrightBrowser = PlaywrightPrefix + "browser";

    /// <summary>
    ///     Header for a screenshot path
    /// </summary>
    public const string PlaywrightScreenshotPath = PlaywrightPrefix + "screenshot_path";

    /// <summary>
    ///     Header for the bytes of a Selenium screenshot
    /// </summary>
    public const string PlaywrightScreenshotBytes = PlaywrightPrefix + "screenshot_bytes";

    /// <summary>
    ///     Header for the saved path of a Playwright video
    /// </summary>
    public const string PlaywrightVideoSavedPath = PlaywrightPrefix + "video_saved_path";

    /// <summary>
    ///     Header for the current path of a Playwright video
    /// </summary>
    public const string PlaywrightVideoCurrentPath = PlaywrightPrefix + "video_current_path";

    /// <summary>
    ///     Identifier for the Playwright test session, used as a context variable or header key.
    /// </summary>
    public const string PlaywrightTestSessionId = PlaywrightPrefix + "test_session_id";
}
