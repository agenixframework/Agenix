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

using System.Configuration;

namespace Agenix.Api.Report;

/// Provides settings for configuring test report generation and management.
/// This class includes settings for managing the test reports such as automatically clearing
/// reports after a test suite is completed, ignoring errors during report generation,
/// and specifying the directory where reports should be stored. Settings are retrieved from
/// configuration properties, environment variables, or default values.
public static class TestReporterSettings
{
    /// Represents the configuration property for automatically clearing reports in the system.
    private static readonly string ReportAutoClearProperty = "agenix.report.auto.clear";

    private static readonly string ReportAutoClearEnv = "AGENIX_REPORT_AUTO_CLEAR";

    private static readonly string ReportIgnoreErrorsProperty = "agenix.report.ignore.errors";
    private static readonly string ReportIgnoreErrorsEnv = "AGENIX_REPORT_IGNORE_ERRORS";

    private static readonly string ReportDirectoryProperty = "agenix.report.directory";
    private static readonly string ReportDirectoryEnv = "AGENIX_REPORT_DIRECTORY";

    /// Get setting if report should automatically clear all test results after finishing the test suite. Default value
    /// is true.
    /// @return true if reports are set to automatically clear; otherwise, false.
    /// /
    public static bool IsAutoClear()
    {
        return bool.Parse(GetPropertyEnvOrDefault(ReportAutoClearEnv, ReportAutoClearProperty, bool.TrueString));
    }

    /// Get setting if report should ignore errors during report generation. Default is true.
    /// @return true if report generation is set to ignore errors; otherwise, false.
    public static bool IsIgnoreErrors()
    {
        return bool.Parse(GetPropertyEnvOrDefault(ReportIgnoreErrorsProperty, ReportIgnoreErrorsEnv, bool.TrueString));
    }

    /// Get the target report directory where to create files.
    /// @return the directory path for report generation.
    /// /
    public static string GetReportDirectory()
    {
        return GetPropertyEnvOrDefault(ReportDirectoryProperty, ReportDirectoryEnv,
            AppDomain.CurrentDomain.BaseDirectory);
    }

    /// <summary>
    ///     Gets in the respective order, a system property, an environment variable, or the default
    /// </summary>
    /// <param name="prop">the name of the system property to get</param>
    /// <param name="env">the name of the environment variable to get</param>
    /// <param name="def">the default value</param>
    /// <returns>the first value encountered, which is not null. May return null if the default value is null.</returns>
    private static string GetPropertyEnvOrDefault(string prop, string env, string def)
    {
        return ConfigurationManager.AppSettings[prop] ??
               Environment.GetEnvironmentVariable(env) ??
               def;
    }
}
