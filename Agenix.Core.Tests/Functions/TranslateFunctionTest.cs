using System.Collections.Generic;
using Agenix.Api.Exceptions;
using Agenix.Core.Functions.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Functions;

public class TranslateFunctionTest : AbstractNUnitSetUp
{
    private readonly TranslateFunction _function = new();

    [Test]
    public void TestFunction()
    {
        var parameters = new List<string> { "H.llo TestFr.mework", "\\.", "a" };
        ClassicAssert.AreEqual(_function.Execute(parameters, Context), "Hallo TestFramework");
    }

    [Test]
    public void TestFunctionWithMoreComplexRegex()
    {
        var parameters = new List<string> { "+110-23673-(279)-96", "[-()]", "" };
        ClassicAssert.AreEqual(_function.Execute(parameters, Context), "+1102367327996");
    }

    [Test]
    public void TestMissingParameters()
    {
        var parameters = new List<string> { "H.llo TestFr.mework", "\\." };
        Assert.Throws<InvalidFunctionUsageException>(() => _function.Execute(parameters, Context));
    }

    [Test]
    public void TestNoParameterUsage()
    {
        Assert.Throws<InvalidFunctionUsageException>(() => _function.Execute(new List<string>(), Context));
    }
}
