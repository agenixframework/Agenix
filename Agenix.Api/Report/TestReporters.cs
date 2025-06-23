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

namespace Agenix.Api.Report;

/// Represents a class responsible for handling test reporting and listening to test events.
/// Implements the ITestListener, ITestSuiteListener, and ITestReporterAware interfaces to manage the test lifecycle and reports.
public class TestReporters : ITestListener, ITestSuiteListener, ITestReporterAware
{
    /** List of test listeners **/
    private readonly List<ITestReporter> _testReporters = [];

    /**
     * Collect test results for overall result overview at the very end of test execution
     */
    private TestResults _testResults = new();

    /**
     * Should clear test results for each test suite
     */
    public bool AutoClear = AgenixSettings.ReportAutoClear();

    /// Invoked when the execution of an individual test case starts.
    /// This method is called to notify that a specific test case has begun execution.
    /// It allows for any setup, logging, or reporting mechanisms to be initialized
    /// specifically for this test case.
    /// <param name="test">The test case that is starting.</param>
    public void OnTestStart(ITestCase test)
    {
    }

    /// Handles the completion of an individual test case.
    /// This method is invoked when a test case finishes its execution.
    /// If the executed test case has a result, the result is added to the
    /// internal collection of test results.
    /// <param name="test">The test case that has finished executing.</param>
    public void OnTestFinish(ITestCase test)
    {
        if (test.GetTestResult() != null)
        {
            _testResults.AddResult(test.GetTestResult());
        }
    }

    /// Handles the event when a test case completes successfully.
    /// This method is invoked after a test case execution finishes without any errors.
    /// It allows for any necessary clean-up or state update related to the successful completion of a test.
    /// <param name="test">The test case that has been successfully completed.</param>
    public void OnTestSuccess(ITestCase test)
    {
    }

    /// Triggered when a test case fails.
    /// This method handles any necessary reporting or logging when a test does not pass.
    /// <param name="test">The test case that failed.</param>
    /// <param name="cause">The exception that caused the failure.</param>
    public void OnTestFailure(ITestCase test, Exception cause)
    {
    }

    /// Called when a test is skipped during the test run.
    /// This method handles the scenario when a test is marked as skipped,
    /// allowing the test reporting mechanism to appropriately record and report
    /// the skipped status of the test case.
    /// <param name="test">The test case that was skipped.</param>
    public void OnTestSkipped(ITestCase test)
    {
    }

    /// Adds a test reporter to the list of test reporters.
    /// <param name="testReporter">The test reporter to be added.</param>
    public void AddTestReporter(ITestReporter testReporter)
    {
        _testReporters.Add(testReporter);
    }

    /// Executes when a test suite starts.
    /// This method initiates the test suite, and if the AutoClear setting is enabled,
    /// it clears any previous test results to ensure a fresh start.
    public void OnStart()
    {
        if (AutoClear)
        {
            _testResults = new TestResults();
        }
    }

    /// Executes when a test suite finishes with failures.
    /// This method is triggered if the test suite encounters any errors during its execution,
    /// and it initiates the generation of test reports.
    /// <param name="cause">The exception that caused the failure.</param>
    public void OnFinishFailure(Exception cause)
    {
        GenerateReports();
    }

    /// Executes when a test suite finishes successfully.
    /// This method is responsible for generating the test reports
    /// once all the tests in the suite have completed without any failures.
    public void OnFinishSuccess()
    {
        GenerateReports();
    }

    /// Executes when a test suite finishes.
    /// This method handles the necessary actions or cleanup procedures required at the end of a test suite execution.
    public void OnFinish()
    {
    }

    /// Executes when there is a failure at the start of a test suite.
    /// This method handles the failure scenario, capturing the exception that caused the failure.
    /// <param name="cause">The exception that triggered the start failure.</param>
    public void OnStartFailure(Exception cause)
    {
    }

    /// Executes when a test suite starts successfully.
    /// This method performs the necessary actions to handle the successful start of a test suite,
    /// such as logging or initializing specific components related to the testing process.
    public void OnStartSuccess()
    {
    }

    /// Retrieves the list of test reporters.
    /// This method returns a collection of ITestReporter instances that are currently registered with the TestReporters class.
    /// <return>A list of ITestReporter instances.</return>
    public List<ITestReporter> GetTestReporters()
    {
        return [.. _testReporters];
    }

    /// Indicates if the test results should be cleared for each test suite.
    /// <return>Returns true if test results should be cleared before starting a new test suite; otherwise, false.</return>
    public bool IsAutoClear()
    {
        return AutoClear;
    }

    /// Sets the AutoClear property to the specified value.
    /// <param name="autoClear">If true, previous test results will be cleared for each test suite.</param>
    public void SetAutoClear(bool autoClear)
    {
        AutoClear = autoClear;
    }

    /// Retrieves the collection of test results.
    /// This method returns the current state of test results, which includes all the results
    /// collected during the test suite's execution.
    public TestResults GetTestResults()
    {
        return _testResults;
    }

    /// Generates reports for all test reporters in the list.
    /// This method iterates through the registered test reporters and invokes their GenerateReport method, passing along
    /// the test results collected during the test execution.
    public void GenerateReports()
    {
        foreach (var reporter in _testReporters)
        {
            reporter.GenerateReport(_testResults);
        }
    }
}
