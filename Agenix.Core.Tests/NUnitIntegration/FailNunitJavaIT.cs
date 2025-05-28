using System;
using Agenix.Api;
using Agenix.Api.Annotations;
using Agenix.Api.Exceptions;
using Agenix.Core.Actions;
using Agenix.NUnit.Runtime.Agenix.NUnit.Attribute;
using NUnit.Framework;

namespace Agenix.Core.Tests.NUnitIntegration;

[NUnitAgenixSupport]
public class FailNunitJavaIT
{
    [AgenixResource]
#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value
    private ITestActionRunner _runner;
#pragma warning restore CS0649 // Field is never assigned to, and will always have its default value

    [Test]
    [Category("ShouldFailGroup")]
    public void FailTest()
    {
        Assert.Throws<TestCaseFailedException>(() =>
            _runner.Run(EchoAction.Builder.Echo("This test should fail because of unknown variable ${foo}")));
    }

    [Test]
    [Category("ShouldFailGroup")]
    public void FailRuntimeExceptionTest()
    {
        Assert.Throws<TestCaseFailedException>(() => _runner.Run(DefaultTestActionBuilder.Action(
            _ => throw new Exception("This test should fail because of unknown variable ${foo}"))));
    }
}
