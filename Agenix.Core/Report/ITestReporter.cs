namespace Agenix.Core.Report;

/// Interface for test reporters.
/// /
public interface ITestReporter
{
    /// Generates a report for the provided test results.
    /// @param testResults The collection of test results for which to generate the report.
    /// /
    void GenerateReport(TestResults testResults);
}