namespace Agenix.Core;

/// Interface for running test cases, which includes functionality for starting and stopping test execution.
/// This interface extends ITestCaseBuilder to provide methods for building test cases
/// and IGherkinTestActionRunner to support behavior-driven development (BDD) style methods.
public interface ITestCaseRunner : ITestCaseBuilder, IGherkinTestActionRunner
{
    /// Starts the test case execution.
    /// /
    void Start();

    /// Stops test case execution.
    void Stop();
}