using Agenix.Core.Actions;
using Agenix.Core.Exceptions;
using NUnit.Framework;

namespace Agenix.Core.NUnitTestProject.Actions;

public class EchoActionTest : AbstractNUnitSetUp
{
    [Test]
    public void TestEchoMessage()
    {
        var echo = new EchoAction.Builder().Message("Hello Agenix!").Build();

        echo.Execute(Context);
    }

    [Test]
    public void TestEchoMessageWithVariables()
    {
        var echo = new EchoAction.Builder().Message("${greeting} Agenix!").Build();
        Context.SetVariable("greeting", "Hello");

        echo.Execute(Context);
    }

    [Test]
    public void TestEchoMessageWithFunctions()
    {
        var echo = new EchoAction.Builder().Message("Today is core:CurrentDate()").Build();

        echo.Execute(Context);
    }

    [Test]
    public void TestEchoMessageWithUnkonwnVariables()
    {
        var echo = new EchoAction.Builder().Message("${greeting} Agenix").Build();

        Assert.Throws<CoreSystemException>(() => echo.Execute(Context));
    }
}