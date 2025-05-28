using Agenix.Api.Exceptions;
using Agenix.Core.Functions.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Functions;

public class LowerCaseFunctionTest : AbstractNUnitSetUp
{
    private readonly LowerCaseFunction _function = new();

    [Test]
    public void TestLowerCaseFunction()
    {
        ClassicAssert.AreEqual(_function.Execute(["1000"], Context), "1000");
        ClassicAssert.AreEqual(_function.Execute(["hallo TestFramework!"], Context),
            "hallo testframework!");
        ClassicAssert.AreEqual(_function.Execute(["Today is: 09.02.2012"], Context),
            "today is: 09.02.2012");
    }

    [Test]
    public void TestNoParameters()
    {
        Assert.Throws<InvalidFunctionUsageException>(() => _function.Execute([], Context));
    }
}
