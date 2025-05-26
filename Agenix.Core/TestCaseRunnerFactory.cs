using System;
using Agenix.Api;
using Agenix.Api.Context;
using Agenix.Api.Exceptions;
using Agenix.Core.Spi;

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

    /** Test runner resource lookup path */
    private const string ResourcePath = "Extension/agenix/test/runner";

    /// Resolves resource paths into types and properties based on a predefined or custom resource base path.
    /// Used to map resource identifiers to corresponding types in the Agenix framework.
    /// /
    private readonly ResourcePathTypeResolver _typeResolver = new(ResourcePath);

    private static readonly TestCaseRunnerFactory Instance = new();

    private TestCaseRunnerFactory()
    {
        // Singleton
    }

    /// <summary>
    /// Retrieves the default implementation of ITestCaseRunnerProvider.
    /// </summary>
    /// <returns>
    /// An instance of ITestCaseRunnerProvider used for creating test case runners.
    /// </returns>
    private ITestCaseRunnerProvider LookupDefault()
    {
        return _typeResolver.Resolve<ITestCaseRunnerProvider>(Default);
    }

    /// <summary>
    /// Attempts to resolve a custom implementation of ITestCaseRunnerProvider.
    /// If the custom implementation cannot be resolved, defaults to the standard implementation.
    /// </summary>
    /// <returns>
    /// An instance of ITestCaseRunnerProvider, either custom or default, used for creating test case runners.
    /// </returns>
    private ITestCaseRunnerProvider LookupCustomOrDefault()
    {
        try
        {
            return _typeResolver.Resolve<ITestCaseRunnerProvider>(Custom);
        }
        catch (Exception)
        {
            return LookupDefault();
        }
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
        var testCaseRunnerProvider = Instance.LookupCustomOrDefault();
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
        var testCaseRunnerProvider = Instance.LookupCustomOrDefault();
        return testCaseRunnerProvider.CreateTestCaseRunner(testCase, context);
    }
}