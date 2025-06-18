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

using System;
using Agenix.Api.Log;
using Agenix.Api.Report;
using Microsoft.Extensions.Logging;

namespace Agenix.Core.Report;

/// <summary>
///     Abstract base class for test reporters, providing common functionality and enforcing the implementation of the
///     Generate method.
/// </summary>
public abstract class AbstractTestReporter : ITestReporter
{
    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILogger Log = LogManager.GetLogger(typeof(AbstractTestReporter));

    /// <summary>
    ///     Indicates whether errors should be ignored during test report creation.
    /// </summary>
    public bool IgnoreErrors { get; set; } = TestReporterSettings.IsIgnoreErrors();

    /// <summary>
    ///     Specifies the directory where the test reports will be stored.
    /// </summary>
    public string ReportDirectory { get; set; } = TestReporterSettings.GetReportDirectory();

    /// Generates a report based on the provided test results.
    /// <param name="testResults">The collection of test results to include in the report.</param>
    public void GenerateReport(TestResults testResults)
    {
        try
        {
            Generate(testResults);
        }
        catch (Exception e)
        {
            if (IgnoreErrors)
            {
                Log.LogError(e, "Failed to create test report");
            }
            else
            {
                throw;
            }
        }
    }

    /// Subclasses must implement this method to generate a test report based on the given test results.
    /// <param name="testResults">The test results to be included in the generated report.</param>
    public abstract void Generate(TestResults testResults);
}
