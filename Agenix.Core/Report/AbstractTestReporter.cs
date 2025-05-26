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
                Log.LogError(e, "Failed to create test report");
            else
                throw;
        }
    }

    /// Subclasses must implement this method to generate a test report based on the given test results.
    /// <param name="testResults">The test results to be included in the generated report.</param>
    public abstract void Generate(TestResults testResults);
}