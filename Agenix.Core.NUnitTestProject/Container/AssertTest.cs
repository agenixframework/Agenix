using System;
using Agenix.Core.Actions;
using Agenix.Core.Exceptions;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using static Agenix.Core.Container.AssertContainer.Builder;
using Assert = NUnit.Framework.Assert;

namespace Agenix.Core.NUnitTestProject.Container;

public class AssertTest : AbstractNUnitSetUp
{
    [Test]
    public void TestAssertDefaultException()
    {
        var assertAction = Assert()
            .Actions(new FailAction.Builder())
            .Build();

        assertAction.Execute(Context);
    }

    [Test]
    public void TestAssertException()
    {
        var exceptionType = typeof(CoreSystemException);

        var assertAction = Assert()
            .Actions(new FailAction.Builder())
            .Exception(exceptionType)
            .Build();

        assertAction.Execute(Context);
    }

    [Test]
    public void TestAssertExceptionMessageCheck()
    {
        var failActionBuilder = new FailAction.Builder()
            .Message("This went wrong!");

        var exceptionType = typeof(CoreSystemException);

        var assertAction = Assert()
            .Actions(failActionBuilder)
            .Exception(exceptionType)
            .Message("This went wrong!")
            .Build();

        assertAction.Execute(Context);
    }

    [Test]
    public void TestVariableSupport()
    {
        Context.SetVariable("message", "This went wrong!");

        var failActionBuilder = new FailAction.Builder()
            .Message("This went wrong!");

        var exceptionType = typeof(CoreSystemException);

        var assertAction = Assert()
            .Actions(failActionBuilder)
            .Exception(exceptionType)
            .Message(Context.GetVariable("${message}"))
            .Build();

        assertAction.Execute(Context);
    }

    [Test]
    public void TestValidationMatcherSupport()
    {
        var failActionBuilder = new FailAction.Builder()
            .Message("This went wrong!");

        var exceptionType = typeof(CoreSystemException);

        var assertAction = Assert()
            .Actions(failActionBuilder)
            .Exception(exceptionType)
            .Message("@Contains('wrong')@")
            .Build();

        assertAction.Execute(Context);
    }

    [Test]
    public void TestAssertExceptionWrongMessageCheck()
    {
        var failActionBuilder = new FailAction.Builder().Message("This went wrong!");

        var exceptionType = typeof(CoreSystemException);

        var assertAction = Assert()
            .Actions(failActionBuilder)
            .Exception(exceptionType)
            .Message("Expected error is something else")
            .Build();

        var exception = Assert.Throws<ValidationException>(() => assertAction.Execute(Context));

        // Expect the test to fail when checking the wrong message content
        StringAssert.Contains("Expected error is something else", exception.Message);
    }

    [Test]
    public void TestMissingException()
    {
        var exceptionType = typeof(CoreSystemException);

        var assertAction = Assert()
            .Actions(new EchoAction.Builder())
            .Exception(exceptionType)
            .Build();

        try
        {
            assertAction.Execute(Context);
            Assert.Fail("Expected CoreRuntimeException to be thrown, but it was not.");
        }
        catch (CoreSystemException)
        {
            // Test passes if CoreRuntimeException is caught
        }
        catch (Exception)
        {
            // Any other exception type, fail the test
            Assert.Fail("Expected CoreRuntimeException, but another exception was thrown.");
        }
    }
}