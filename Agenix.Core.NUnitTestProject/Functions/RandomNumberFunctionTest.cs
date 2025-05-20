using System;
using System.Collections.Generic;
using Agenix.Api.Exceptions;
using Agenix.Core.Functions.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Functions;

public class RandomNumberFunctionTest : AbstractNUnitSetUp
{
    private readonly RandomNumberFunction _function = new();

    [Test]
    public void TestRandomStringFunction()
    {
        var parameters = new List<string> { "3" };

        ClassicAssert.Less(int.Parse(_function.Execute(parameters, Context)), 1000);

        parameters = new List<string> { "3", "false" };
        var generated = _function.Execute(parameters, Context);

        ClassicAssert.LessOrEqual(generated.Length, 3);
        ClassicAssert.Greater(generated.Length, 0);
    }

    [Test]
    public void TestLeadingZeroNumbers()
    {
        var generated = RandomNumberFunction.CheckLeadingZeros("0001", true);
        Console.WriteLine(generated);
        ClassicAssert.Greater(int.Parse(generated.Substring(0, 1)), 0);

        generated = RandomNumberFunction.CheckLeadingZeros("0009", true);
        ClassicAssert.AreEqual(generated.Length, 4);

        generated = RandomNumberFunction.CheckLeadingZeros("00000", true);
        ClassicAssert.AreEqual(generated.Length, 5);
        ClassicAssert.Greater(int.Parse(generated.Substring(0, 1)), 0);
        ClassicAssert.IsTrue(generated.EndsWith("0000"));

        generated = RandomNumberFunction.CheckLeadingZeros("009809", true);
        ClassicAssert.AreEqual(generated.Length, 6);
        ClassicAssert.Greater(int.Parse(generated.Substring(0, 1)), 0);
        ClassicAssert.IsTrue(generated.EndsWith("09809"));

        generated = RandomNumberFunction.CheckLeadingZeros("01209", true);
        ClassicAssert.AreEqual(generated.Length, 5);
        ClassicAssert.Greater(int.Parse(generated.Substring(0, 1)), 0);
        ClassicAssert.IsTrue(generated.EndsWith("1209"));

        generated = RandomNumberFunction.CheckLeadingZeros("1209", true);
        ClassicAssert.AreEqual(generated.Length, 4);
        ClassicAssert.AreEqual(generated, "1209");

        generated = RandomNumberFunction.CheckLeadingZeros("00000", false);
        ClassicAssert.AreEqual(generated.Length, 1);
        ClassicAssert.AreEqual(generated, "0");

        generated = RandomNumberFunction.CheckLeadingZeros("0009", false);
        ClassicAssert.AreEqual(generated.Length, 1);
        ClassicAssert.AreEqual(generated, "9");

        generated = RandomNumberFunction.CheckLeadingZeros("01209", false);
        ClassicAssert.AreEqual(generated.Length, 4);
        ClassicAssert.AreEqual(generated, "1209");

        generated = RandomNumberFunction.CheckLeadingZeros("1209", false);
        ClassicAssert.AreEqual(generated.Length, 4);
        ClassicAssert.AreEqual(generated, "1209");
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