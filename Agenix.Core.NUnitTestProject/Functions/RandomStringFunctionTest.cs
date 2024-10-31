using System.Collections.Generic;
using Agenix.Core.Exceptions;
using Agenix.Core.Functions.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Functions;

public class RandomStringFunctionTest : AbstractNUnitSetUp
{
    private readonly RandomStringFunction _function = new();

    [Test]
    public void TestRandomStringFunction()
    {
        var parameters = new List<string> { "3" };

        ClassicAssert.AreEqual(_function.Execute(parameters, Context).Length, 3);

        parameters = ["3", RandomStringFunction.Uppercase];

        ClassicAssert.AreEqual(_function.Execute(parameters, Context).Length, 3);

        parameters = ["3", RandomStringFunction.Lowercase];

        ClassicAssert.AreEqual(_function.Execute(parameters, Context).Length, 3);

        parameters = ["3", RandomStringFunction.Mixed];

        ClassicAssert.AreEqual(_function.Execute(parameters, Context).Length, 3);

        parameters = new List<string> { "3", "UNKNOWN" };

        ClassicAssert.AreEqual(_function.Execute(parameters, Context).Length, 3);
    }

    [Test]
    public void TestWithNumbers()
    {
        var parameters = new List<string> { "10", RandomStringFunction.Uppercase, "true" };

        ClassicAssert.AreEqual(_function.Execute(parameters, Context).Length, 10);

        parameters = new List<string> { "10", RandomStringFunction.Lowercase, "true" };

        ClassicAssert.AreEqual(_function.Execute(parameters, Context).Length, 10);

        parameters = new List<string> { "10", RandomStringFunction.Mixed, "true" };

        ClassicAssert.AreEqual(_function.Execute(parameters, Context).Length, 10);

        parameters = new List<string> { "10", "UNKNOWN", "true" };

        ClassicAssert.AreEqual(_function.Execute(parameters, Context).Length, 10);
    }

    [Test]
    public void TestWrongParameterUsage()
    {
        Assert.Throws<InvalidFunctionUsageException>(() => _function.Execute(new List<string> { "-1" }, Context));
    }

    [Test]
    public void TestNoParameterUsage()
    {
        Assert.Throws<InvalidFunctionUsageException>(() => _function.Execute(new List<string>(), Context));
    }

    [Test]
    public void TestTooManyParameters()
    {
        var parameters = new List<string> { "3", RandomStringFunction.Uppercase, "true", "too much" };
        Assert.Throws<InvalidFunctionUsageException>(() => _function.Execute(parameters, Context));
    }
}