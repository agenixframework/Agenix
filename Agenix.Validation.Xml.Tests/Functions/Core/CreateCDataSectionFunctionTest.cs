using System.Runtime.InteropServices.JavaScript;
using Agenix.Api.Exceptions;
using Agenix.Api.Functions;
using Agenix.Core.Functions;
using Agenix.Validation.Xml.Functions.Core;
using NUnit.Framework;

namespace Agenix.Validation.Xml.Tests.Functions.Core;

[TestFixture]
public class CreateCDataSectionFunctionTest : AbstractNUnitSetUp
{
    private readonly CreateCDataSectionFunction _function = new();

    [Test]
    public void TestFunction()
    {
        const string xmlFragment = "<foo><bar>I like Agenix!</bar></foo>";
        const string resultXml = "<![CDATA[<foo><bar>I like Agenix!</bar></foo>]]>";

        Assert.That(_function.Execute([xmlFragment], Context), Is.EqualTo(resultXml));
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

        Assert.That(functionLookup.ContainsKey("cdataSection"), Is.True);
        Assert.That(functionLookup["cdataSection"].GetType(), Is.EqualTo(typeof(CreateCDataSectionFunction)));

        var defaultLibrary = new DefaultFunctionLibrary();
        Assert.That(defaultLibrary.GetFunction("cdataSection").GetType(), Is.EqualTo(typeof(CreateCDataSectionFunction)));
    }
}
