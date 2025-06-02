using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Agenix.Api;
using Agenix.Api.Exceptions;
using Agenix.Core.Actions;
using Agenix.Core.Functions.Core;
using Moq;
using NUnit.Framework;
using TestContext = Agenix.Api.Context.TestContext;
using static Agenix.Core.Actions.DefaultTestActionBuilder;

namespace Agenix.Core.Tests.TestCase;

public class DefaultTestCaseTest : AbstractNUnitSetUp
{
    private DefaultTestCase _fixture;

    [SetUp]
    public void Initialize()
    {
        _fixture = new DefaultTestCase();
    }

    [Test]
    public void FailEmptyTestResult()
    {
        Assert.That(_fixture.GetTestResult(), Is.Null);

        var throwableMock = new Mock<Exception>().Object;
        _fixture.Fail(throwableMock);

        var result = _fixture.GetTestResult();

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<TestResult>());
        Assert.That(result.Cause, Is.EqualTo(throwableMock));
        Assert.That(result.Result, Is.EqualTo(TestResult.RESULT.FAILURE));
        Assert.That(result.Duration, Is.Not.Null);
    }

    [Test]
    public void FailOverridesOtherTestResult()
    {
        _fixture.SetTestResult(TestResult.Success("failOverridesOtherTestResult", GetType().Name));

        var throwableMock = new Mock<Exception>().Object;
        _fixture.Fail(throwableMock);

        var result = _fixture.GetTestResult();

        Assert.That(result, Is.Not.Null);
        Assert.That(result, Is.InstanceOf<TestResult>());
        Assert.That(result.Cause, Is.EqualTo(throwableMock));
        Assert.That(result.Result, Is.EqualTo(TestResult.RESULT.FAILURE));
        Assert.That(result.Duration, Is.Not.Null);
    }

    [Test]
    public void TestExecution()
    {
        _fixture.SetName("MyTestCase");

        _fixture.AddTestAction(new EchoAction.Builder().Build());

        _fixture.Execute(Context);
        _fixture.Finish(Context);

        VerifyDurationHasBeenMeasured(_fixture.GetTestResult());
    }

    [Test]
    public void TestWaitForFinish()
    {
        _fixture.SetName("MyTestCase");

        _fixture.AddTestAction(new ConcreteAsyncTestAction
        {
            DoExecuteAsyncFunc = _ =>
            {
                try
                {
                    Thread.Sleep(500);
                }
                catch (Exception e)
                {
                    throw new AgenixSystemException(e.Message);
                }

                return Task.CompletedTask;
            }
        });
        var echoAction = new EchoAction.Builder().Build();

        _fixture.AddTestAction(echoAction);

        _fixture.Execute(Context);
        _fixture.Finish(Context);

        var duration = VerifyDurationHasBeenMeasured(_fixture.GetTestResult());
        if (duration < TimeSpan.FromMilliseconds(500))
        {
            Assert.Fail(
                "TestResult / Duration should be more than 500 ms, because that's how long the async action takes!");
        }
    }

    [Test]
    public void TestWaitForFinishTimeout()
    {
        _fixture.SetName("MyTestCase");
        _fixture.Timeout = 500;

        var echoAction = new EchoAction.Builder().Build();

        _fixture.AddTestAction(new ConcreteAsyncTestAction
        {
            DoExecuteAsyncFunc = _ =>
            {
                try
                {
                    Thread.Sleep(1000);
                }
                catch (Exception e)
                {
                    throw new AgenixSystemException(e.Message);
                }

                return Task.CompletedTask;
            }
        });
        _fixture.AddTestAction(echoAction);

        _fixture.Execute(Context);

        var exception = Assert.Throws(typeof(TestCaseFailedException), () => { _fixture.Finish(Context); });

        Debug.Assert(exception != null, nameof(exception) + " != null");
        Assert.That(exception.Message,
            Is.EqualTo("Failed to wait for the test container to finish properly - timeout exceeded"));
    }

    [Test]
    public void TestExecutionWithVariables()
    {
        _fixture.SetName("MyTestCase");
        Dictionary<string, object> variables = new()
        {
            { "name", "Agenix" },
            { "framework", "${name}" },
            { "hello", "core:Concat('Hello ', ${name}, '!')" },
            { "goodbye", "Goodbye ${name}!" },
            { "welcome", "Welcome ${name}, today is core:CurrentDate()!" }
        };
        _fixture.SetVariableDefinitions(variables);

        _fixture.AddTestAction(Action(context =>
        {
            Assert.That(context.GetVariables()[AgenixSettings.TestNameVariable()], Is.EqualTo("MyTestCase"));
            Assert.That(context.GetVariables()[AgenixSettings.TestNameSpaceVariable()],
                Is.EqualTo(typeof(DefaultTestCase).Namespace));
            Assert.That(context.GetVariable("${name}"), Is.EqualTo("Agenix"));
            Assert.That(context.GetVariable("${framework}"), Is.EqualTo("Agenix"));
            Assert.That(context.GetVariable("${hello}"), Is.EqualTo("Hello Agenix!"));
            Assert.That(context.GetVariable("${goodbye}"), Is.EqualTo("Goodbye Agenix!"));
            Assert.That(context.GetVariable("${welcome}"), Is.EqualTo("Welcome Agenix, today is " +
                                                                      new CurrentDateFunction().Execute([], Context) +
                                                                      "!"));
        }));

        _fixture.Execute(Context);
        _fixture.Finish(Context);
        VerifyDurationHasBeenMeasured(_fixture.GetTestResult());
    }

    [Test]
    public void TestUnknownVariable()
    {
        _fixture.SetName("MyTestCase");
        const string message = "Hello TestFramework!";

        _fixture.SetVariableDefinitions(new Dictionary<string, object> { { "text", message } });

        _fixture.AddTestAction(Action(context => Assert.That(context.GetVariable("${unknown}"), Is.EqualTo(message))));

        Assert.Throws(typeof(TestCaseFailedException), () =>
        {
            _fixture.Execute(Context);
            _fixture.Finish(Context);
            VerifyDurationHasBeenMeasured(_fixture.GetTestResult());
        });
    }

    [Test]
    public void TestExceptionInContext()
    {
        _fixture.SetName("MyTestCase");

        _fixture.AddTestAction(Action(context =>
            context.AddException(new AgenixSystemException("This failed in forked action"))).Build());

        _fixture.AddTestAction(new EchoAction.Builder().Message("Everything is fine!").Build());

        var exception = Assert.Throws(typeof(TestCaseFailedException), () => { _fixture.Execute(Context); });
        _fixture.Finish(Context);

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Is.EqualTo("This failed in forked action"));
    }

    [Test]
    public void TestExceptionInContextInFinish()
    {
        _fixture.SetName("MyTestCase");

        _fixture.AddTestAction(Action(context =>
            context.AddException(new AgenixSystemException("This failed in forked action"))).Build());

        _fixture.Execute(Context);
        var exception = Assert.Throws(typeof(TestCaseFailedException), () => { _fixture.Finish(Context); });

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Is.EqualTo("This failed in forked action"));
    }

    [Test]
    public void TestFinalActions()
    {
        _fixture.SetName("MyTestCase");
        _fixture.AddTestAction(new EchoAction.Builder().Message("Everything is fine!").Build());
        _fixture.AddFinalAction(new EchoAction.Builder().Build());

        _fixture.Execute(Context);
        _fixture.Finish(Context);

        VerifyDurationHasBeenMeasured(_fixture.GetTestResult());
    }

    /// <summary>
    ///     Verifies that the duration has been measured for the given test result.
    /// </summary>
    /// <param name="fixture">The test result to verify.</param>
    /// <returns>The duration of the test result.</returns>
    private static TimeSpan VerifyDurationHasBeenMeasured(TestResult fixture)
    {
        Assert.That(fixture, Is.Not.Null);
        Assert.That(fixture.Duration, Is.Not.Null);
        return fixture.Duration;
    }

    private class ConcreteAsyncTestAction : AbstractAsyncTestAction
    {
        public Func<TestContext, Task> DoExecuteAsyncFunc { get; init; }

        public override Task DoExecuteAsync(TestContext context)
        {
            return DoExecuteAsyncFunc(context);
        }
    }
}
