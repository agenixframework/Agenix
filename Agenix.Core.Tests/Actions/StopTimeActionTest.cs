using System.Threading;
using Agenix.Core.Actions;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Actions;

public class StopTimeActionTest : AbstractNUnitSetUp
{
    [Test]
    public void TestDefaultTimeline()
    {
        var stopTime = new StopTimeAction.Builder().Build();

        ClassicAssert.IsFalse(Context.GetVariables().ContainsKey(StopTimeAction.DefaultTimeLineId));
        ClassicAssert.IsFalse(Context.GetVariables()
            .ContainsKey(StopTimeAction.DefaultTimeLineId + StopTimeAction.DefaultTimeLineValueSuffix));

        stopTime.Execute(Context);
        ClassicAssert.IsTrue(Context.GetVariables().ContainsKey(StopTimeAction.DefaultTimeLineId));
        ClassicAssert.IsTrue(Context.GetVariables()
            .ContainsKey(StopTimeAction.DefaultTimeLineId + StopTimeAction.DefaultTimeLineValueSuffix));
        ClassicAssert.AreEqual(
            Context.GetVariable<long>(StopTimeAction.DefaultTimeLineId + StopTimeAction.DefaultTimeLineValueSuffix), 0L);
        Thread.Sleep(100);
        stopTime.Execute(Context);
        ClassicAssert.IsTrue(Context.GetVariables().ContainsKey(StopTimeAction.DefaultTimeLineId));
        ClassicAssert.IsTrue(Context.GetVariables()
            .ContainsKey(StopTimeAction.DefaultTimeLineId + StopTimeAction.DefaultTimeLineValueSuffix));
        ClassicAssert.IsTrue(
            Context.GetVariable<long>(StopTimeAction.DefaultTimeLineId + StopTimeAction.DefaultTimeLineValueSuffix) >= 100L);
        Thread.Sleep(100);
        stopTime.Execute(Context);
        ClassicAssert.IsTrue(Context.GetVariables().ContainsKey(StopTimeAction.DefaultTimeLineId));
        ClassicAssert.IsTrue(Context.GetVariables()
            .ContainsKey(StopTimeAction.DefaultTimeLineId + StopTimeAction.DefaultTimeLineValueSuffix));
        ClassicAssert.IsTrue(
            Context.GetVariable<long>(StopTimeAction.DefaultTimeLineId + StopTimeAction.DefaultTimeLineValueSuffix) >= 200L);
    }

    [Test]
    public void TestCustomTimeline()
    {
        var stopTime = new StopTimeAction.Builder()
            .Id("stopMe")
            .Suffix("_time")
            .Build();

        ClassicAssert.IsFalse(Context.GetVariables().ContainsKey("stopMe"));
        ClassicAssert.IsFalse(Context.GetVariables().ContainsKey("stopMe_time"));

        stopTime.Execute(Context);
        ClassicAssert.IsTrue(Context.GetVariables().ContainsKey("stopMe"));
        ClassicAssert.IsTrue(Context.GetVariables().ContainsKey("stopMe_time"));
        ClassicAssert.AreEqual(Context.GetVariable<long>("stopMe_time"), 0L);

        Thread.Sleep(100);
        stopTime.Execute(Context);
        ClassicAssert.IsTrue(Context.GetVariables().ContainsKey("stopMe"));
        ClassicAssert.IsTrue(Context.GetVariables().ContainsKey("stopMe_time"));
        ClassicAssert.IsTrue(Context.GetVariable<long>("stopMe_time") >= 100L);
        Thread.Sleep(100);
        stopTime.Execute(Context);
        ClassicAssert.IsTrue(Context.GetVariables().ContainsKey("stopMe"));
        ClassicAssert.IsTrue(Context.GetVariables().ContainsKey("stopMe_time"));
        ClassicAssert.IsTrue(Context.GetVariable<long>("stopMe_time") >= 200L);
    }

    [Test]
    public void TestMultipleTimelines()
    {
        var stopTime1 = new StopTimeAction.Builder()
            .Id("stopThem")
            .Build();

        var stopTime2 = new StopTimeAction.Builder()
            .Id("stopUs")
            .Build();

        ClassicAssert.IsFalse(Context.GetVariables().ContainsKey("stopThem"));
        ClassicAssert.IsFalse(Context.GetVariables().ContainsKey("stopThem_VALUE"));
        ClassicAssert.IsFalse(Context.GetVariables().ContainsKey("stopUs"));
        ClassicAssert.IsFalse(Context.GetVariables().ContainsKey("stopUs_VALUE"));

        stopTime1.Execute(Context);
        ClassicAssert.IsTrue(Context.GetVariables().ContainsKey("stopThem"));
        ClassicAssert.IsTrue(Context.GetVariables().ContainsKey("stopThem_VALUE"));
        ClassicAssert.IsFalse(Context.GetVariables().ContainsKey("stopUs"));
        ClassicAssert.IsFalse(Context.GetVariables().ContainsKey("stopUs_VALUE"));
        ClassicAssert.AreEqual(Context.GetVariable<long>("stopThem_VALUE"), 0L);

        Thread.Sleep(100);
        stopTime2.Execute(Context);
        ClassicAssert.IsTrue(Context.GetVariables().ContainsKey("stopThem"));
        ClassicAssert.IsTrue(Context.GetVariables().ContainsKey("stopThem_VALUE"));
        ClassicAssert.IsTrue(Context.GetVariables().ContainsKey("stopUs"));
        ClassicAssert.IsTrue(Context.GetVariables().ContainsKey("stopUs_VALUE"));
        ClassicAssert.AreEqual(Context.GetVariable<long>("stopThem_VALUE"), 0L);
        ClassicAssert.AreEqual(Context.GetVariable<long>("stopUs_VALUE"), 0L);

        Thread.Sleep(100);
        stopTime1.Execute(Context);
        stopTime2.Execute(Context);
        ClassicAssert.IsTrue(Context.GetVariables().ContainsKey("stopThem"));
        ClassicAssert.IsTrue(Context.GetVariables().ContainsKey("stopThem_VALUE"));
        ClassicAssert.IsTrue(Context.GetVariables().ContainsKey("stopUs"));
        ClassicAssert.IsTrue(Context.GetVariables().ContainsKey("stopUs_VALUE"));
        ClassicAssert.IsTrue(Context.GetVariable<long>("stopThem_VALUE") >= 200L);
        ClassicAssert.IsTrue(Context.GetVariable<long>("stopUs_VALUE") >= 100L);
    }
}
