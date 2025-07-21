using System;
using Agenix.Api.Exceptions;
using Agenix.Core.Functions.Core;
using NUnit.Framework;

namespace Agenix.Core.Tests.Functions;

public class AbsoluteFunctionTest : AbstractNUnitSetUp
{
    private readonly AbsoluteFunction _function = new();

    [Test]
    public void TestFunction()
    {
        Assert.That(_function.Execute(["-0"], Context), Is.EqualTo("0"));
        Assert.That(_function.Execute(["2.0"], Context), Is.EqualTo("2.0"));
        Assert.That(_function.Execute(["2"], Context), Is.EqualTo("2"));
        Assert.That(_function.Execute(["2.5"], Context), Is.EqualTo("2.5"));
        Assert.That(_function.Execute(["-2.0"], Context), Is.EqualTo("2.0"));
        Assert.That(_function.Execute(["-2"], Context), Is.EqualTo("2"));
        Assert.That(_function.Execute(["-2.5"], Context), Is.EqualTo("2.5"));
    }

    [Test]
    public void TestWrongParameterUsage()
    {
        Assert.Throws<FormatException>(() => _function.Execute(["no digit"], Context));
    }

    [Test]
    public void TestNoParameters()
    {
        Assert.Throws<InvalidFunctionUsageException>(() => _function.Execute([], Context));
    }
}
