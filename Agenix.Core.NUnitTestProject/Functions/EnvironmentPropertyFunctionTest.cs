using System.Collections.Generic;
using Agenix.Api.Exceptions;
using Agenix.Api.Functions;
using Agenix.Core.Functions;
using Agenix.Core.Functions.Core;
using Microsoft.Extensions.Configuration;
using Moq;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Functions;

public class EnvironmentPropertyFunctionTest : AbstractNUnitSetUp
{
    private readonly EnvironmentPropertyFunction _function = new();
    private readonly Mock<IConfiguration> _mockEnvironment = new();

    [SetUp]
    public void SetUpMethod()
    {
        _function.SetEnvironment(_mockEnvironment.Object);
    }

    [Test]
    public void TestFunction()
    {
        _mockEnvironment.Setup(env => env["foo.property"]).Returns("Agenix rocks!");
        ClassicAssert.AreEqual("Agenix rocks!", _function.Execute(["foo.property"], null));
    }

    [Test]
    public void TestFunctionDefaultValue()
    {
        ClassicAssert.AreEqual("This is a default",
            _function.Execute(["bar.property", "This is a default"], null));
    }

    [Test]
    public void TestPropertyNotFound()
    {
        Assert.Throws<AgenixSystemException>(() => _function.Execute(["bar.property"], null));
    }

    [Test]
    public void TestNoParameters()
    {
        Assert.Throws<InvalidFunctionUsageException>(() => _function.Execute([], null));
    }

    [Test]
    public void ShouldLookupFunction()
    {
        ClassicAssert.True(IFunction.Lookup().ContainsKey("env"));
        ClassicAssert.AreEqual(typeof(EnvironmentPropertyFunction), IFunction.Lookup()["env"].GetType());
        ClassicAssert.AreEqual(typeof(EnvironmentPropertyFunction),
            new DefaultFunctionLibrary().GetFunction("env").GetType());
    }
}