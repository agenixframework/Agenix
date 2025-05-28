using Agenix.Api.Container;
using Agenix.Core.Actions;
using Agenix.Core.Container;
using Moq;
using NUnit.Framework;

namespace Agenix.Core.Tests.Actions;

public class StopTimerActionTest : AbstractNUnitSetUp
{
    [Test]
    public void ShouldStopSpecificTimer()
    {
        const string timerId = "timer#1";

        var mock = new Mock<IStopTimer>();
        var timer = mock.Object;
        Context.RegisterTimer(timerId, timer);

        var stopTimer = new StopTimerAction.Builder()
            .Id(timerId)
            .Build();

        Assert.That(stopTimer.TimerId, Is.EqualTo(timerId));
        stopTimer.Execute(Context);

        mock.Verify(x => x.StopTimer(), Times.Once);
    }

    [Test]
    public void ShouldStopAllTimers()
    {
        const string timerId1 = "timer#1";
        const string timerId2 = "timer#2";

        var mock1 = new Mock<IStopTimer>();
        var timer1 = mock1.Object;
        Context.RegisterTimer(timerId1, timer1);

        var mock2 = new Mock<IStopTimer>();
        var timer2 = mock2.Object;
        Context.RegisterTimer(timerId2, timer2);

        var stopTimer = new StopTimerAction.Builder().Build();
        stopTimer.Execute(Context);

        mock1.Verify(x => x.StopTimer(), Times.Once);
        mock2.Verify(x => x.StopTimer(), Times.Once);
    }

    [Test]
    public void ShouldNotFailWhenStoppingTimerWithUnknownId()
    {
        var stopTimer = new StopTimerAction.Builder()
            .Id("some-unknown-timer")
            .Build();
        stopTimer.Execute(Context);
    }
}
