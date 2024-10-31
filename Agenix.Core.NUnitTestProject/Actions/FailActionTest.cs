using Agenix.Core.Actions;
using Agenix.Core.Exceptions;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Actions;

[TestFixture]
public class FailActionTest : AbstractNUnitSetUp
{
    [Test]
    public void TestFailStandardMessage()
    {
        var fail = new FailAction.Builder().Build();
        try
        {
            fail.Execute(Context);
        }
        catch (CoreSystemException e)
        {
            ClassicAssert.AreEqual("Generated error to interrupt test execution", e.Message);
            return;
        }

        Assert.Fail("Missing CoreSystemException");
    }

    [Test]
    public void TestFailCustomizedMessage()
    {
        var fail = new FailAction.Builder()
            .Message("Failed because I said so")
            .Build();
        try
        {
            fail.Execute(Context);
        }
        catch (CoreSystemException e)
        {
            ClassicAssert.AreEqual("Failed because I said so", e.Message);
            return;
        }

        Assert.Fail("Missing CoreSystemException");
    }

    [Test]
    public void TestFailCustomizedMessageWithVariables()
    {
        var fail = new FailAction.Builder()
            .Message("Failed because I said so, ${text}")
            .Build();

        Context.SetVariable("text", "period!");

        try
        {
            fail.Execute(Context);
        }
        catch (CoreSystemException e)
        {
            ClassicAssert.AreEqual("Failed because I said so, period!", e.Message);
            return;
        }

        Assert.Fail("Missing CoreSystemException");
    }
}