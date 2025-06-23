using Agenix.Api.Exceptions;
using Agenix.Core.Actions;
using NUnit.Framework;

namespace Agenix.Core.Tests.Actions;

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
        var echo = new EchoAction.Builder().Message("Today is agenix:CurrentDate()").Build();

        echo.Execute(Context);
    }

    [Test]
    public void TestEchoMessageWithUnkonwnVariables()
    {
        var echo = new EchoAction.Builder().Message("${greeting} Agenix").Build();

        Assert.Throws<AgenixSystemException>(() => echo.Execute(Context));
    }
}
