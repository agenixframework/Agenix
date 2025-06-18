using System;
using Agenix.Api;
using Agenix.Api.Report;

namespace Agenix.Core.Report;

/// <summary>
///     Basic implementation of <see cref="ITestListener" /> interface so that subclasses must not implement
///     all methods but only overwrite some listener methods.
/// </summary>
public abstract class AbstractTestListener : ITestListener
{
    public virtual void OnTestFailure(ITestCase test, Exception cause) { }

    public virtual void OnTestSkipped(ITestCase test) { }

    public virtual void OnTestStart(ITestCase test) { }

    public void OnTestFinish(ITestCase test)
    {
    }

    public virtual void OnTestSuccess(ITestCase test) { }

    public virtual void OnTestExecutionEnd(ITestCase test) { }
}
