using System.Threading;
using Agenix.Core.Actions;
using Agenix.Core.Exceptions;
using log4net;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using Timer = Agenix.Core.Container.Timer;

namespace Agenix.Core.NUnitTestProject.Container;

public class TimerTest : AbstractNUnitSetUp
{
    private const int DefaultRepeatCount = 3;
    private const long DefaultInterval = 50L;

    /// <summary>
    ///     Logger.
    /// </summary>
    private static readonly ILog Log = LogManager.GetLogger(typeof(TimerTest));

    private Mock<ITestAction> _action;

    [SetUp]
    public void SetupMethod()
    {
        _action = new Mock<ITestAction>();
    }

    [Test]
    public void ShouldSuccessfullyRunTimerWithNestedAction()
    {
        // Arrange - Reset mock before usage
        _action.Reset();

        var timer = CreateDefaultTimerWithNestedAction(false, _action.Object);


        // Act - Execute the timer
        timer.Execute(Context);

        // Assert - Check the timer's execution and index
        AssertTimerIndex(DefaultRepeatCount, timer);
        _action.Verify(a => a.Execute(Context), Times.Exactly(DefaultRepeatCount));
    }

    [Test]
    public void ShouldSuccessfullyRunTimerWithNestedActionThatTakesLongerThanTimerInterval()
    {
        // Arrange - Reset mock actions
        _action.Reset();

        var timer = CreateDefaultTimerWithNestedAction(false, _action.Object, GetSleepAction());

        // Act - Execute the timer
        timer.Execute(Context);

        // Assert - Check timer index and action invocation count
        AssertTimerIndex(DefaultRepeatCount, timer);
        _action.Verify(a => a.Execute(Context), Times.Exactly(DefaultRepeatCount));
    }

    [Test]
    public void ShouldFailPropagatingErrorUpCallStack()
    {
        // Arrange
        var timer = CreateDefaultTimerWithNestedActionThatFails(false);

        // Act & Assert - Verify exception is thrown
        Assert.Throws<CoreSystemException>(() => timer.Execute(Context));
    }

    [Test]
    public void ShouldSuccessfullyRunForkedTimerWithNestedAction()
    {
        // Arrange - Reset the mock
        _action.Reset();

        var timer = CreateDefaultTimerWithNestedAction(true, _action.Object);

        // Act - Execute the timer
        timer.Execute(Context);

        AllowForkedTimerToComplete(DefaultInterval * DefaultRepeatCount);

        // Assert - Verify that the action executed the expected number of times
        AssertTimerIndex(DefaultRepeatCount, timer);
        _action.Verify(a => a.Execute(Context), Times.Exactly(DefaultRepeatCount));
    }

    [Test]
    public void ShouldCompleteSuccessfullyForForkedTimerWithNestedActionThatFails()
    {
        // Arrange
        var timer = CreateDefaultTimerWithNestedActionThatFails(true);

        // Act - Execute the timer
        timer.Execute(Context);

        AllowForkedTimerToComplete(DefaultInterval);

        // Assert - Verify completion and exception handling
        AssertTimerIndex(1, timer);
        ClassicAssert.IsNotNull(timer.TimerException);
    }

    private Timer CreateTimerWithNestedAction(int repeatCount, long interval, bool forked, params ITestAction[] actions)
    {
        return new Timer.Builder()
            .Interval(interval)
            .RepeatCount(repeatCount)
            .Fork(forked)
            .Actions(actions)
            .Build();
    }

    private Timer CreateDefaultTimerWithNestedActionThatFails(bool forked)
    {
        return CreateDefaultTimerWithNestedAction(forked, GetFailAction());
    }

    private Timer CreateDefaultTimerWithNestedAction(bool forked, params ITestAction[] testActions)
    {
        return CreateTimerWithNestedAction(DefaultRepeatCount, DefaultInterval, forked, testActions);
    }

    /// <summary>
    ///     Creates and returns a new instance of the FailAction class with a preset error message.
    /// </summary>
    /// <returns>
    ///     A FailAction instance representing a predefined failure action, designed to simulate an error condition in a test.
    /// </returns>
    private FailAction GetFailAction()
    {
        return new FailAction.Builder().Message("Something nasty happened").Build();
    }

    /// <summary>
    ///     Creates and returns a new instance of the SleepAction class, configured to pause execution for a specified
    ///     duration.
    /// </summary>
    /// <returns>
    ///     A SleepAction instance that, when executed, causes the current thread to sleep for 200 milliseconds.
    /// </returns>
    private SleepAction GetSleepAction()
    {
        return new SleepAction.Builder()
            .Milliseconds(200L)
            .Build();
    }

    /// <summary>
    ///     Pauses the current thread to allow time for a forked timer operation to complete.
    /// </summary>
    /// <param name="sleepTime">
    ///     The duration in milliseconds for which the thread should pause, plus an additional 1000
    ///     milliseconds.
    /// </param>
    private void AllowForkedTimerToComplete(long sleepTime)
    {
        try
        {
            Thread.Sleep((int)(sleepTime + 1000));
        }
        catch (ThreadInterruptedException e)
        {
            Log.Error("Interrupted while waiting for forked timer", e);
        }
    }

    /// <summary>
    ///     Validates that the current index value for a given timer matches the expected value.
    /// </summary>
    /// <param name="expectedValue">The expected index value for the timer.</param>
    /// <param name="timer">The timer instance whose current index value is being validated.</param>
    private void AssertTimerIndex(int expectedValue, Timer timer)
    {
        var actualValue = Context.GetVariable(timer.TimerId + Timer.IndexSuffix);
        ClassicAssert.AreEqual(expectedValue.ToString(), actualValue);
    }
}