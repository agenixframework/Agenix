namespace Agenix.Core;

/// <summary>
///     The TestCaseRunnerFactory class provides methods to create instances of ITestCaseRunner.
///     It includes functionality to create test case runners based on a given TestContext or TestCase.
/// </summary>
public class TestCaseRunnerFactory
{
    /**
     * The key for the default Agenix test case runner provider
     */
#pragma warning disable CS0414 // Field is assigned but its value is never used
    private static readonly string Default = "default";
#pragma warning restore CS0414 // Field is assigned but its value is never used

    /**
     * The key for a custom test case runner provider
     */
#pragma warning disable CS0414 // Field is assigned but its value is never used
    private static readonly string Custom = "custom";
#pragma warning restore CS0414 // Field is assigned but its value is never used

    private static readonly TestCaseRunnerFactory Instance = new();

    private TestCaseRunnerFactory()
    {
        // Singleton
    }

    /// <summary>
    ///     Retrieves the default implementation of ITestCaseRunnerProvider.
    /// </summary>
    /// <returns>
    ///     An instance of DefaultTestCaseRunner.DefaultTestCaseRunnerProvider used for running test cases.
    /// </returns>
    private ITestCaseRunnerProvider LookupDefault()
    {
        return new DefaultTestCaseRunner.DefaultTestCaseRunnerProvider();
    }

    /// <summary>
    ///     Creates an ITestCaseRunner instance for the specified TestContext.
    /// </summary>
    /// <param name="context">The TestContext in which the test case runner will operate.</param>
    /// <returns>
    ///     An ITestCaseRunner instance configured with the provided TestContext.
    /// </returns>
    public static ITestCaseRunner CreateRunner(TestContext context)
    {
        var testCaseRunnerProvider = Instance.LookupDefault();
        return testCaseRunnerProvider.CreateTestCaseRunner(context);
    }

    /// <summary>
    ///     Creates a test case runner for the specified test case and context.
    /// </summary>
    /// <param name="testCase">The test case to be executed by the runner.</param>
    /// <param name="context">The context in which the test case will be executed.</param>
    /// <returns>An instance of ITestCaseRunner used to run the specified test case.</returns>
    public static ITestCaseRunner CreateRunner(ITestCase testCase, TestContext context)
    {
        var testCaseRunnerProvider = Instance.LookupDefault();
        return testCaseRunnerProvider.CreateTestCaseRunner(testCase, context);
    }
}