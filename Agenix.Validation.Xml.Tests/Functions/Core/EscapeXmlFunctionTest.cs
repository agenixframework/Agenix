using Agenix.Api.Exceptions;
using Agenix.Api.Functions;
using Agenix.Core.Functions;
using Agenix.Validation.Xml.Functions.Core;
using NUnit.Framework;

namespace Agenix.Validation.Xml.Tests.Functions.Core;

[TestFixture]
public class EscapeXmlFunctionTest : AbstractNUnitSetUp
{
    private readonly EscapeXmlFunction _function = new();

    [Test]
    public void TestFunction()
    {
        const string xmlFragment = "<foo><bar>Yes, I like Agenix!</bar></foo>";
        const string escapedXml = "&lt;foo&gt;&lt;bar&gt;Yes, I like Agenix!&lt;/bar&gt;&lt;/foo&gt;";

        Assert.That(_function.Execute([xmlFragment], Context), Is.EqualTo(escapedXml));
    }

    [Test]
    public void TestNoParameters()
    {
        Assert.That(() => _function.Execute([], Context),
            Throws.TypeOf<InvalidFunctionUsageException>());
    }

    [Test]
    public void ShouldLookupFunction()
    {
        var functionLookup = IFunction.Lookup();

        Assert.That(functionLookup.ContainsKey("escapeXml"), Is.True);
        Assert.That(functionLookup["escapeXml"].GetType(), Is.EqualTo(typeof(EscapeXmlFunction)));

        var defaultLibrary = new DefaultFunctionLibrary();
        Assert.That(defaultLibrary.GetFunction("escapeXml").GetType(), Is.EqualTo(typeof(EscapeXmlFunction)));
    }
}
