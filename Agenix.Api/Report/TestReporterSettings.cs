#region License

// MIT License
//
// Copyright (c) 2025 Agenix
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

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
