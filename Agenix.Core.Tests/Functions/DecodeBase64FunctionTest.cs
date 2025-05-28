using System.Collections.Generic;
using Agenix.Api.Exceptions;
using Agenix.Core.Functions.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Functions;

public class DecodeBase64FunctionTest : AbstractNUnitSetUp
{
    private readonly DecodeBase64Function _function = new();

    [Test]
    public void TestFunction()
    {
        ClassicAssert.AreEqual(_function.Execute(new List<string> { "Zm9v" }, Context), "foo");
    }

    [Test]
    public void TestNoParameterUsage()
    {
        Assert.Throws<InvalidFunctionUsageException>(() => _function.Execute([], Context));
    }
}
