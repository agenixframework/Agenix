using Agenix.Api.Exceptions;
using Agenix.Core.Actions;
using NUnit.Framework;

namespace Agenix.Core.Tests.Actions;

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
        Assert.Throws<AgenixSystemException>(() =>
        {
            var trace = new TraceVariablesAction.Builder()
                .Variable("myVariable")
                .Build();
            trace.Execute(Context);
        });
    }
}
