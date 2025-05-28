using System;
using Agenix.Core.Actions;
using Agenix.Core.Util;
using NUnit.Framework;

namespace Agenix.Core.Tests.Actions;

public class SleepActionTest : AbstractNUnitSetUp
{
    [Test]
    public void TestSleepDuration()
    {
        var sleep = new SleepAction.Builder()
            .Time(TimeSpan.FromMilliseconds(200))
            .Build();

        sleep.Execute(Context);
    }

    [Test]
    public void TestSleep()
    {
        var sleep = new SleepAction.Builder()
            .Milliseconds(100L)
            .Build();

        sleep.Execute(Context);
    }

    [Test]
    public void TestSleepVariablesSupport()
    {
        var sleep = new SleepAction.Builder()
            .Milliseconds("${time}")
            .Build();

        Context.SetVariable("time", "100");

        sleep.Execute(Context);
    }

    [Test]
    public void TestSleepDecimalValueSupport()
    {
        var sleep = new SleepAction.Builder()
            .Time("500.0", ScheduledExecutor.TimeUnit.MILLISECONDS)
            .Build();

        sleep.Execute(Context);

        /*sleep = new SleepAction.Builder()
            .Time("0.5", ScheduledExecutor.TimeUnit.SECONDS)
            .Build();

        sleep.Execute(Context);*/

        /*sleep = new SleepAction.Builder()
            .Time("0.01", ScheduledExecutor.TimeUnit.MINUTES)
            .Build();

        sleep.Execute(Context);*/
    }

    [Test]
    public void TestSleepLegacy()
    {
        var sleep = new SleepAction.Builder()
            .Seconds(0.1)
            .Build();

        sleep.Execute(Context);
    }

    [Test]
    public void TestSleepLegacyVariablesSupport()
    {
        var sleep = new SleepAction.Builder()
            .Time("${time}", ScheduledExecutor.TimeUnit.SECONDS)
            .Build();

        Context.SetVariable("time", "1");

        sleep.Execute(Context);
    }
}
