using System.Collections.Generic;
using Agenix.Api.Exceptions;
using Agenix.Core.Functions.Core;
using NUnit.Framework;

namespace Agenix.Core.Tests.Functions;

public class AvgFunctionTest : AbstractNUnitSetUp
{
    private AvgFunction _function;

    [SetUp]
    public void SetUp()
    {
        _function = new AvgFunction();
    }

    [Test]
    public void TestFunction()
    {
        var parameters = new List<string> { "1", "2", "3" };

        Assert.That(_function.Execute(parameters, Context), Is.EqualTo("2"));
    }

    [Test]
    public void TestWrongParameterUsage()
    {
        Assert.Throws<InvalidFunctionUsageException>(() => _function.Execute(["no digit"], Context));
    }

    [Test]
    public void TestNoParameters()
    {
        Assert.Throws<InvalidFunctionUsageException>(() => _function.Execute([], Context));
    }
}
