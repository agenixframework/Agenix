namespace Agenix.Core;

/// <summary>
///     Interface for providing TestCaseRunner.
/// </summary>
public interface ITestCaseRunnerProvider
{
    /// Creates a TestCaseRunner which runs the given TestContext.
    /// @param context Test execution context.
    /// @return A new instance of ITestCaseRunner to run tests with the provided context.
    /// /
    ITestCaseRunner CreateTestCaseRunner(ITestCase testCase, TestContext context);

    /// Creates a TestCaseRunner which runs the given {@link TestCase} and the given {@link TestContext}.
    /// @param testCase The test case to be run by the TestCaseRunner.
    /// @param context The context in which the test case will be run.
    /// @return An instance of ITestCaseRunner that runs the specified test case within the given context.
    /// /
    ITestCaseRunner CreateTestCaseRunner(TestContext context);
}