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

namespace Agenix.Selenium.Endpoint;

/// <summary>
///     Contains constant header names used for Selenium-related actions and operations.
///     This class cannot be instantiated and provides static constants for header identification.
/// </summary>
public static class SeleniumHeaders
{
    /// <summary>
    ///     Special header prefix for Selenium related actions
    /// </summary>
    public const string SeleniumPrefix = "selenium_";

    /// <summary>
    ///     Header for browser identification
    /// </summary>
    public const string SeleniumBrowser = SeleniumPrefix + "browser";

    /// <summary>
    ///     Header for alert text content
    /// </summary>
    public const string SeleniumAlertText = SeleniumPrefix + "alert_text";

    /// <summary>
    ///     Header for active window identification
    /// </summary>
    public const string SeleniumActiveWindow = SeleniumPrefix + "active_window";

    /// <summary>
    ///     Header for last window identification
    /// </summary>
    public const string SeleniumLastWindow = SeleniumPrefix + "last_window";

    /// <summary>
    ///     Header for JavaScript errors collection
    /// </summary>
    public const string SeleniumJsErrors = SeleniumPrefix + "js_errors";

    /// <summary>
    ///     Header for screenshot data
    /// </summary>
    public const string SeleniumScreenshot = SeleniumPrefix + "screenshot";

    /// <summary>
    ///     Header for download file information
    /// </summary>
    public const string SeleniumDownloadFile = SeleniumPrefix + "download_file";
}
