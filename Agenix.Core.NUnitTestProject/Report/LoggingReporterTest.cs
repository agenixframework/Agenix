using System;
using System.Globalization;
using System.Reflection;
using Agenix.Api;
using Agenix.Api.Exceptions;
using Agenix.Api.Report;
using Agenix.Core.Actions;
using Agenix.Core.Report;
using log4net;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Agenix.Core.NUnitTestProject.Report;

//[Platform(Exclude = "MacOsX", Reason = "Only runs on non-Linux/ Unix platforms.")]
[Ignore("It need to refactoring using Logging Wrapper")]
public class LoggingReporterTest
{
    private EchoAction _echo;

    private LoggingReporter _fixture;
    private Mock<ILogger> _logger;

    private DefaultTestCase _test;


    [SetUp]
    public void BeforeMethod()
    {
        _logger = new Mock<ILogger>();

        _test = new DefaultTestCase();
        _test.SetName("SampleIT");
        _test.SetNamespaceName("agenix.core");

        _echo = new EchoAction.Builder().Build();
        _echo.SetDescription("Test echo action");
        _test.AddTestAction(_echo);

        _fixture = new LoggingReporter();

        SetField(_fixture, "_logger", _logger.Object);
    }

    [Test]
    [Ignore("Should be investigate why it's failing while executing all tests")]
    public void OnStartPrintsBannerExactlyOnce()
    {
        // Print agenix information on startup
        _fixture.OnStart();

        _logger.Verify(logger => logger.LogInformation(It.Is<string>(s => s.StartsWith("A G E N I X  T E S T S"))), Times.Once);

        // Do not print it continuously
        _logger.Invocations.Clear();
        _fixture.OnStart();

        _logger.Verify(logger => logger.LogInformation(It.Is<string>(s => s.StartsWith("A G E N I X  T E S T S"))), Times.Never);
    }

    [Test]
    public void TestLoggingReporterSuccess()
    {
        _fixture.OnStart();
        _fixture.OnStartSuccess();
        _fixture.OnTestStart(_test);
        _fixture.OnTestActionStart(_test, _echo);
        _fixture.OnTestActionFinish(_test, _echo);
        _fixture.OnTestFinish(_test);
        _fixture.OnTestSuccess(_test);
        _fixture.OnFinish();
        _fixture.OnFinishSuccess();

        _logger.Verify(l => l.LogInformation("TEST SUCCESS SampleIT (agenix.core)"));

        var testResults = new TestResults();
        testResults.AddResult(TestResult.Success("TestLoggingReporterSuccess-1", GetType().Name)
            .WithDuration(TimeSpan.FromMilliseconds(111)));
        testResults.AddResult(TestResult.Success("TestLoggingReporterSuccess-2", GetType().Name)
            .WithDuration(TimeSpan.FromMilliseconds(222)));

        _fixture.Generate(testResults);

        _logger.Verify(l => l.LogInformation("SUCCESS (   111 ms) TestLoggingReporterSuccess-1"));
        _logger.Verify(l => l.LogInformation("SUCCESS (   222 ms) TestLoggingReporterSuccess-2"));

        VerifyResultSummaryLog(2, 2, 0, 333);

        _logger.Verify(l => l.LogDebug(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void TestLoggingReporterFailed()
    {
        var nestedException = new AgenixSystemException("I am the final boss.");
        var cause = new AgenixSystemException("Failed!", nestedException);

        _fixture.OnStart();
        _fixture.OnStartSuccess();
        _fixture.OnTestStart(_test);
        _fixture.OnTestActionStart(_test, _echo);
        _fixture.OnTestFinish(_test);
        _fixture.OnTestFailure(_test, cause);
        _fixture.OnFinish();
        _fixture.OnFinishSuccess();

        //_logger.Verify(l => l.LogError(cause, "TEST FAILED SampleIT <agenix.core> Nested exception is: "));
        _logger
            .Setup(x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception, string>)It.IsAny<object>()))
            .Verifiable();
        

        var testResults = new TestResults();
        testResults.AddResult(TestResult.Failed("TestLoggingReporterFailed-1", GetType().Name, cause)
            .WithDuration(TimeSpan.FromMilliseconds(1234)));
        testResults.AddResult(TestResult.Failed("TestLoggingReporterFailed-2", GetType().Name, nestedException)
            .WithDuration(TimeSpan.FromMilliseconds(2345)));
        var customErrorMessage = "custom error message";
        testResults.AddResult(TestResult.Failed("TestLoggingReporterFailed-3", GetType().Name, customErrorMessage)
            .WithDuration(TimeSpan.FromMilliseconds(3456)));

        _fixture.Generate(testResults);

        _logger.Verify(l => l.LogInformation(
            It.Is<string>(message => message.Contains("FAILURE (  1234 ms) TestLoggingReporterFailed-1"))));


        //_logger.Verify(l => l.LogInformation("FAILURE (  1234 ms) TestLoggingReporterFailed-1"));
        //_logger.Verify(l => l.LogInformation("FAILURE (  2345 ms) TestLoggingReporterFailed-2"));
        _logger.Verify(l => l.LogInformation($"\tCaused by: {nestedException.GetType().Name}: {nestedException.Message}"),
            Times.Exactly(1));
        _logger.Verify(l => l.LogInformation("FAILURE (  3456 ms) TestLoggingReporterFailed-3"));
        _logger.Verify(l => l.LogInformation($"\tCaused by: {customErrorMessage}"));

        VerifyResultSummaryLog(3, 0, 3, 7035);

        _logger.Verify(l => l.LogDebug(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void TestLoggingReporterMiscellaneous()
    {
        var testResults = new TestResults();
        testResults.AddResult(TestResult.Success("TestLoggingReporterMiscellaneous-1", GetType().Name)
            .WithDuration(TimeSpan.FromSeconds(1)));
        testResults.AddResult(TestResult.Failed("TestLoggingReporterMiscellaneous-2", GetType().Name, default(string))
            .WithDuration(TimeSpan.FromMilliseconds(22)));

        _fixture.Generate(testResults);

        _logger.Verify(l => l.LogInformation("SUCCESS (  1000 ms) TestLoggingReporterMiscellaneous-1"));
        _logger.Verify(l => l.LogInformation("FAILURE (    22 ms) TestLoggingReporterMiscellaneous-2"));
        _logger.Verify(l => l.LogInformation("\tCaused by: Unknown error"));

        VerifyResultSummaryLog(2, 1, 1, 1022);

        _logger.Verify(l => l.LogDebug(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void TestLoggingReporterSkipped()
    {
        _fixture.OnStart();
        _fixture.OnStartSuccess();
        _fixture.OnTestStart(_test);
        _fixture.OnTestFinish(_test);
        _fixture.OnTestSuccess(_test);
        _fixture.OnTestSkipped(new DefaultTestCase());
        _fixture.OnFinish();
        _fixture.OnFinishSuccess();

        var testResults = new TestResults();
        testResults.AddResult(TestResult.Skipped("testLoggingReporterSkipped", GetType().Name));

        _fixture.Generate(testResults);

        _logger.Verify(l => l.LogInformation("SKIP (     0 ms) testLoggingReporterSkipped"));

        VerifyResultSummaryLog(0, 0, 0, 0);

        _logger.Verify(l => l.LogDebug(It.IsAny<string>()), Times.Never);
    }

    [Test]
    public void TestLoggingReporterBeforeSuiteFailed()
    {
        var exception = new AgenixSystemException("Failed!");

        _fixture.OnStart();
        _fixture.OnStartFailure(exception);
        _fixture.OnFinish();
        _fixture.OnFinishSuccess();

        var testResults = new TestResults();
        _fixture.Generate(testResults);
    }

    [Test]
    public void TestLoggingReporterAfterSuiteFailed()
    {
        _fixture.OnStart();
        _fixture.OnStartSuccess();
        _fixture.OnTestStart(_test);
        _fixture.OnTestActionStart(_test, _echo);
        _fixture.OnTestActionFinish(_test, _echo);
        _fixture.OnTestFinish(_test);
        _fixture.OnTestSuccess(_test);
        _fixture.OnFinish();
        _fixture.OnFinishFailure(new AgenixSystemException("Failed!"));

        var testResults = new TestResults();
        _fixture.Generate(testResults);
    }

    private void VerifyResultSummaryLog(int total, int success, int failed, double performance)
    {
        _logger.Verify(l => l.LogInformation("TOTAL:\t\t" + total));
        _logger.Verify(l => l.LogInformation("SUCCESS:\t\t" + success + " (" + CalculatePercentage(total, success) + "%)"));
        _logger.Verify(l => l.LogInformation("FAILED:\t\t" + failed + " (" + CalculatePercentage(total, failed) + "%)"));
        _logger.Verify(l => l.LogInformation("PERFORMANCE:\t" + performance + " ms"));
    }

    private string CalculatePercentage(int total, int successCount)
    {
        if (total == 0) return "0.0";

        var percentage = (double)successCount / total * 100;
        return string.Format(CultureInfo.InvariantCulture, "{0:0.0}", percentage);
    }

    public static void SetField(object targetObject, string fieldName, object value)
    {
        var field = targetObject.GetType().GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Static);
        if (field != null)
            field.SetValue(targetObject, value);
        else
            throw new ArgumentException($"Field '{fieldName}' not found on {targetObject.GetType().FullName}");
    }
}