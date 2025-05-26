using System;
using Agenix.Api;
using Agenix.Api.Common;
using Agenix.Api.Container;
using Agenix.Api.Context;
using Agenix.Api.Log;
using Agenix.Api.Message;
using Agenix.Api.Report;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Agenix.Core.Report;

/// <summary>
///     Simple logging reporter printing test start and ending to the console/ logger. This class provides an option for
///     disablement, allowing you to suppress logging for specific instances and delegate the logging to another facility,
///     which could potentially be a subclass of LoggingReporter. It's important to note that when an instance of this
///     class is disabled, it will not perform any logging, irrespective of the severity level. Implementation note: The
///     disablement of the reporter is achieved by using a log4net. helpers. NOPLogger, meaning that this class should
///     primarily focus on logging operations and not extend beyond that functionality.
/// </summary>
/// <param name="logger"></param>
public class LoggingReporter : AbstractTestReporter, IMessageListener, ITestSuiteListener,
    ITestListener, ITestActionListener
{
    /// Logger
    /// /
    private static ILogger _logger = LogManager.GetLogger(typeof(LoggingReporter));

    /// Inbound message logger
    private static ILogger _inboundMessageLogger = LogManager.GetLogger("Logger.Message_IN");

    /// The inbound message logger used when logging is enabled for inbound messages.
    private static readonly ILogger EnabledInboundMessageLogger = _inboundMessageLogger;

    /// Outbound message logger
    private static ILogger _outboundMessageLogger = LogManager.GetLogger("Logger.Message_OUT");

    /// The logger used for outbound messages when the reporter is enabled.
    private static readonly ILogger EnabledOutboundMessageLogger = _outboundMessageLogger;

    /// The standard logger used when the reporter is enabled
    private static readonly ILogger EnabledLog = _logger;

    private static bool _isInitialized;

    private static readonly ILogger NoOpLogger = NullLogger.Instance;
    

    /// <summary>
    ///     Handles an inbound message by logging its details using a debug logger.
    /// </summary>
    /// <param name="message">The incoming message to be logged.</param>
    /// <param name="context">
    ///     The context within which the message is being processed, providing additional environment or
    ///     configuration details.
    /// </param>
    public void OnInboundMessage(IMessage message, TestContext context)
    {
        _inboundMessageLogger.LogDebug(message.Print(context));
    }

    /// <summary>
    ///     Handles the event when an outbound message is sent.
    ///     Logs the message details using the Message_OUT logger.
    /// </summary>
    /// <param name="message">The outbound message being sent.</param>
    /// <param name="context">The context of the current test, providing additional execution details.</param>
    public void OnOutboundMessage(IMessage message, TestContext context)
    {
        _outboundMessageLogger.LogDebug(message.Print(context));
    }

    /// <summary>
    ///     Executes the necessary actions at the start of a test action, including logging activity for debugging purposes.
    /// </summary>
    /// <param name="testCase">The test case that contains the action being started.</param>
    /// <param name="testAction">The specific test action that is starting.</param>
    public void OnTestActionStart(ITestCase testCase, ITestAction testAction)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            NewLine();
            Debug(testCase.IsIncremental()
                ? $"TEST STEP {testCase.GetExecutedActions().Count + 1}: {testAction.Name ?? testAction.GetType().Name}"
                : $"TEST STEP {testCase.GetActionIndex(testAction) + 1}/{testCase.GetActionCount()}: {testAction.Name ?? testAction.GetType().Name}");

            if (testAction is ITestActionContainer container)
                Debug($"TEST ACTION CONTAINER with {container.GetActionCount()} embedded actions");

            if (testAction is not IDescribed described || string.IsNullOrWhiteSpace(described.GetDescription())) return;
            Debug("");
            Debug(described.GetDescription());
            Debug("");
        }
    }

    /// <summary>
    ///     Handles the logic executed when a test action finishes successfully.
    /// </summary>
    /// <param name="testCase">The test case containing the action that has finished.</param>
    /// <param name="testAction">The specific action within the test case that has finished execution.</param>
    public void OnTestActionFinish(ITestCase testCase, ITestAction testAction)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            NewLine();

            var duration = FormatDurationString(testCase);
            Debug(testCase.IsIncremental()
                ? $"TEST STEP {testCase.GetExecutedActions().Count + 1} SUCCESS{duration}"
                : $"TEST STEP {testCase.GetActionIndex(testAction) + 1}/{testCase.GetActionCount()} SUCCESS{duration}");
        }
    }

    /// <summary>
    ///     Handles the event when a test action is skipped during the execution of a test case.
    /// </summary>
    /// <param name="testCase">The test case containing the action that was skipped.</param>
    /// <param name="testAction">The test action that was skipped.</param>
    public void OnTestActionSkipped(ITestCase testCase, ITestAction testAction)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            NewLine();
            Debug(testCase.IsIncremental()
                ? $"SKIPPING TEST STEP {testCase.GetExecutedActions().Count + 1}"
                : $"SKIPPING TEST STEP {testCase.GetActionIndex(testAction) + 1}/{testCase.GetActionCount()}");
            Debug($"TEST ACTION {testAction.Name ?? testAction.GetType().Name} SKIPPED");
        }
    }

    /// <summary>
    ///     Handles the actions to be performed when a test case fails.
    /// </summary>
    /// <param name="testCase">The test case that failed.</param>
    /// <param name="cause">The exception that caused the test case to fail.</param>
    public void OnTestFailure(ITestCase testCase, Exception cause)
    {
        NewLine();

        var duration = FormatDurationString(testCase);
        Error($"TEST FAILED {testCase.Name} <{testCase.GetNamespaceName()}>{duration} Nested exception is: ", cause);

        Separator();
        NewLine();
    }

    /// <summary>
    ///     Logs the event when a test is skipped, if debugging is enabled.
    /// </summary>
    /// <param name="test">The test case that was skipped.</param>
    public void OnTestSkipped(ITestCase test)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            NewLine();
            Separator();
            Debug($"SKIPPING TEST: {test.Name}");
            NewLine();
        }
    }

    /// <summary>
    ///     Invoked when a test starts, potentially logging the start of the test if debug mode is enabled.
    /// </summary>
    /// <param name="test">The test case that is starting.</param>
    public void OnTestStart(ITestCase test)
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            NewLine();
            Separator();
            Debug($"STARTING TEST {test.Name} <{test.GetNamespaceName()}>");
            NewLine();
        }
    }

    /// <summary>
    ///     Handles the completion of a test case.
    /// </summary>
    /// <param name="test">The test case that has finished execution.</param>
    public void OnTestFinish(ITestCase test)
    {
        // do nothing
    }

    /// <summary>
    ///     Handles the event when a test case is successfully completed.
    /// </summary>
    /// <param name="test">The test case that has successfully completed.</param>
    public void OnTestSuccess(ITestCase test)
    {
        NewLine();

        var duration = FormatDurationString(test);
        Info($"TEST SUCCESS {test.Name} ({test.GetNamespaceName()}){duration}");

        Separator();
        NewLine();
    }

    /// <summary>
    ///     Invoked after the completion of the test suite to perform logging operations.
    /// </summary>
    public void OnFinish()
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            NewLine();
            Separator();
            Debug("AFTER TEST SUITE");
            NewLine();
        }
    }

    /// <summary>
    ///     Invoked at the start of the test suite. Initializes necessary components and logs startup messages.
    /// </summary>
    public void OnStart()
    {
        if (!_isInitialized)
        {
            PrintBanner();

            _isInitialized = true;
        }

        Separator();

        if (!_logger.IsEnabled(LogLevel.Debug)) return;
        Debug("BEFORE TEST SUITE");
        NewLine();
    }

    /// <summary>
    ///     Handles the failure of a test suite after its completion.
    /// </summary>
    /// <param name="cause">The exception that caused the test suite to fail after finishing.</param>
    public void OnFinishFailure(Exception cause)
    {
        NewLine();
        Error("AFTER TEST SUITE: FAILED");
        Separator();
        NewLine();
    }

    /// <summary>
    ///     Handles post-test suite operations when the test suite completes successfully.
    /// </summary>
    public void OnFinishSuccess()
    {
        if (_logger.IsEnabled(LogLevel.Debug))
        {
            NewLine();
            Debug("AFTER TEST SUITE: SUCCESS");
            Separator();
            NewLine();
        }
    }

    /// <summary>
    ///     Handles the scenario where the startup of the test suite encounters an error.
    /// </summary>
    /// <param name="cause">The exception that caused the startup failure.</param>
    public void OnStartFailure(Exception cause)
    {
        NewLine();
        Error("BEFORE TEST SUITE: FAILED");
        Separator();
        NewLine();
    }

    /// <summary>
    ///     Executes when the test suite starts successfully, providing debug information if enabled.
    /// </summary>
    public void OnStartSuccess()
    {
        if (IsDebugEnabled())
        {
            NewLine();
            Debug("BEFORE TEST SUITE: SUCCESS");
            Separator();
            NewLine();
        }
    }

    /// <summary>
    ///     Formats the duration of the test case into a string representation.
    /// </summary>
    /// <param name="test">The test case from which to retrieve the duration.</param>
    /// <returns>
    ///     A string representation of the test case duration, enclosed in parentheses, or an empty string if the duration
    ///     is null.
    /// </returns>
    private static string FormatDurationString(ITestCase test)
    {
#pragma warning disable CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'
        return test.GetTestResult() != null && test.GetTestResult().Duration != null
#pragma warning restore CS8073 // The result of the expression is always the same since a value of this type is never equal to 'null'
            ? $" ({test.GetTestResult().Duration.ToString()}) "
            : "";
    }

    /// <summary>
    ///     Generates a comprehensive log report of the test results, providing details
    ///     about overall success, failure, skipped tests, and performance metrics.
    /// </summary>
    /// <param name="testResults">The test results to be processed and reported.</param>
    public override void Generate(TestResults testResults)
    {
        NewLine();
        Info("AGENIX TEST RESULTS");
        NewLine();

        testResults.DoWithResults(testResult =>
        {
            Info(ToFormattedTestResult(testResult));

            if (testResult.IsFailed())
                Info(
                    testResult.Cause != null
                    && !string.IsNullOrWhiteSpace(testResult.Cause.Message)
                        ? $"\tCaused by: {testResult.Cause.GetType().Name}: {testResult.Cause.Message}"
                        : $"\tCaused by: {testResult.ErrorMessage ?? "Unknown error"}");
        });

        NewLine();

        Info($"TOTAL:\t\t{testResults.GetFailed() + testResults.GetSuccess()}");
        Info($"SUCCESS:\t\t{testResults.GetSuccess()} ({testResults.GetSuccessPercentageFormatted()}%)");
        Info($"FAILED:\t\t{testResults.GetFailed()} ({testResults.GetFailedPercentageFormatted()}%)");
        Debug($"SKIPPED:\t\t{testResults.GetSkipped()} ({testResults.GetSkippedPercentageFormatted()}%)");
        Info($"PERFORMANCE:\t{(long)testResults.GetTotalDuration().TotalMilliseconds} ms");

        NewLine();

        Separator();
    }

    /// <summary>
    ///     Converts a TestResult object into a formatted string representation.
    /// </summary>
    /// <param name="testResult">The TestResult instance containing the details of a test execution.</param>
    /// <returns>
    ///     A formatted string summarizing the test result, including the result status, duration in milliseconds, and
    ///     test name.
    /// </returns>
    private string ToFormattedTestResult(TestResult testResult)
    {
        return $"{testResult.Result} ({testResult.Duration.TotalMilliseconds,6:G0} ms) {testResult.TestName}";
    }

    /// <summary>
    ///     Prints the Agenix test banner to the log for display purposes.
    /// </summary>
    private void PrintBanner()
    {
        Info("\n   ('-.                   ('-.       .-') _       ) (`-.      \n  " +
             "( OO ).-.             _(  OO)     ( OO ) )       ( OO ).    \n  " +
             "/ . --. /  ,----.    (,------.,--./ ,--,' ,-.-')(_/.  \\_)-. \n  " +
             "| \\-.  \\  '  .-./-')  |  .---'|   \\ |  |\\ |  |OO)\\  `.'  /  " +
             "\n.-'-'  |  | |  |_( O- ) |  |    |    \\|  | )|  |  \\ \\     /\\  \n " +
             "\\| |_.'  | |  | .--, \\(|  '--. |  .     |/ |  |(_/  \\   \\ |  " +
             "\n  |  .-.  |(|  | '. (_/ |  .--' |  |\\    | ,|  |_.' .'    \\_) " +
             "\n  |  | |  | |  '--'  |  |  `---.|  | \\   |(_|  |   /  .'.  \\  " +
             "\n  `--' `--'  `------'   `------'`--'  `--'  `--'  '--'   '--' \n");

        NewLine();
        Info($"A G E N I X  T E S T S  {AgenixVersion.Version}");
        NewLine();
    }

    /// <summary>
    ///     Determines whether debug-level logging is enabled for the logger associated with this reporter.
    /// </summary>
    /// <returns>True if debug-level logging is enabled; otherwise, false.</returns>
    protected bool IsDebugEnabled()
    {
        return _logger.IsEnabled(LogLevel.Debug);
    }

    /// <summary>
    ///     Logs a debug-level message if debug-level logging is enabled for the associated logger.
    /// </summary>
    /// <param name="line">The message to log at the debug level.</param>
    protected void Debug(string line)
    {
        if (_logger.IsEnabled(LogLevel.Debug)) _logger.LogDebug(line);
    }

    /// <summary>
    ///     Logs an error-level message along with an exception object using the associated logger.
    /// </summary>
    /// <param name="line">The error message to log.</param>
    /// <param name="cause">The exception associated with the error to log.</param>
    protected void Error(string line, Exception cause)
    {
        _logger.LogError(cause, line);
    }

    /// <summary>
    ///     Logs an error-level message using the associated logger.
    /// </summary>
    /// <param name="line">The error message to log.</param>
    protected void Error(string line)
    {
        _logger.LogError(line);
    }

    /// <summary>
    ///     Logs an informational-level message using the associated logger.
    /// </summary>
    /// <param name="line">The informational message to log.</param>
    protected void Info(string line)
    {
        _logger.LogInformation(line);
    }

    /// <summary>
    ///     Logs an empty informational-level message using the associated logger to denote a new line.
    /// </summary>
    private void NewLine()
    {
        Info("");
    }

    /// <summary>
    ///     Logs a predefined separator line as an informational message using the associated logger.
    /// </summary>
    private void Separator()
    {
        Info("------------------------------------------------------------------------");
    }

    /**
     * Sets the enablement state of the reporter.
     */
    public void SetEnabled(bool enabled)
    {
        if (enabled)
        {
            _logger = EnabledLog;
            _inboundMessageLogger = EnabledInboundMessageLogger;
            _outboundMessageLogger = EnabledOutboundMessageLogger;
        }
        else
        {
            _logger = NoOpLogger;
            _inboundMessageLogger = NoOpLogger;
            _outboundMessageLogger = NoOpLogger;
        }
    }
    
}