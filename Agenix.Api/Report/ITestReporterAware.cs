namespace Agenix.Api.Report;

/// Interface for classes that support adding test reporters.
public interface ITestReporterAware
{
    /// <summary>
    ///     Adds a new test reporter.
    /// </summary>
    /// <param name="testReporter">The test reporter instance to add.</param>
    void AddTestReporter(ITestReporter testReporter);
}