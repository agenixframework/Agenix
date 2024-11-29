using Agenix.Core.Actions;
using Agenix.Core.Exceptions;
using NUnit.Framework;

namespace Agenix.Core.NUnitTestProject.Actions;

public class TraceVariablesActionTest : AbstractNUnitSetUp
{
    [Test]
    public void TestTraceVariables()
    {
        var trace = new TraceVariablesAction.Builder()
            .Build();
        trace.Execute(Context);
    }

    [Test]
    public void TestTraceSelectedVariables()
    {
        Context.SetVariable("myVariable", "traceMe");

        var trace = new TraceVariablesAction.Builder()
            .Variable("myVariable")
            .Build();
        trace.Execute(Context);
    }

    [Test]
    public void TestTraceUnknownVariable()
    {
        Assert.Throws<CoreSystemException>(() =>
        {
            var trace = new TraceVariablesAction.Builder()
                .Variable("myVariable")
                .Build();
            trace.Execute(Context);
        });
    }
}