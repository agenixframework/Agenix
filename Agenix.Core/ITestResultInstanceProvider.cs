using System;
using Agenix.Api;

namespace Agenix.Core;

/// <summary>
///     Provides methods to create instances of TestResult for different outcomes of a test case.
///     Implementors may decide to create TestResults with specific parameters derived from the TestCase.
/// </summary>
public interface ITestResultInstanceProvider
{
    /// <summary>
    ///     Creates a successful <see cref="TestResult" /> instance for the given test case.
    /// </summary>
    /// <param name="testCase">The test case for which the success result should be created.</param>
    /// <returns>A <see cref="TestResult" /> instance indicating the test case has succeeded.</returns>
    TestResult CreateSuccess(ITestCase testCase);

    /// <summary>
    ///     Creates a failed <see cref="TestResult" /> instance for the given test case.
    /// </summary>
    /// <param name="testCase">The test case for which the failure result should be created.</param>
    /// <param name="ex">The exception that caused the test case to fail.</param>
    /// <returns>A <see cref="TestResult" /> instance indicating the test case has failed.</returns>
    TestResult CreateFailed(ITestCase testCase, Exception ex);

    /// <summary>
    ///     Creates a skipped <see cref="TestResult" /> instance for the given test case.
    /// </summary>
    /// <param name="testCase">The test case for which the skipped result should be created.</param>
    /// <returns>A <see cref="TestResult" /> instance indicating the test case has been skipped.</returns>
    TestResult CreateSkipped(ITestCase testCase);
}