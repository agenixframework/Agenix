using System.Collections.Generic;
using Agenix.Api.Exceptions;
using Agenix.Core.Functions.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Functions;

/// <summary>
///     Test the UpperCaseFunction function
/// </summary>
public class UpperCaseFunctionTest : AbstractNUnitSetUp
{
    private readonly UpperCaseFunction _upperCaseFunction = new();

    [Test]
    public void TestUpperCaseFunction()
    {
        ClassicAssert.AreEqual(_upperCaseFunction.Execute(new List<string> { "1000" }, Context), "1000");
        ClassicAssert.AreEqual(_upperCaseFunction.Execute(new List<string> { "hallo TestFramework!" }, Context),
            "HALLO TESTFRAMEWORK!");
        ClassicAssert.AreEqual(_upperCaseFunction.Execute(new List<string> { "Today is: 09.02.2012" }, Context),
            "TODAY IS: 09.02.2012");
    }

    [Test]
    public void TestNoParameters()
    {
        Assert.Throws<InvalidFunctionUsageException>(() =>
            _upperCaseFunction.Execute(new List<string>(), Context)
        );
    }
}