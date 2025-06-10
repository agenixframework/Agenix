using Agenix.Api.Exceptions;
using Agenix.Api.Functions;
using Agenix.Core.Functions;
using Agenix.Validation.Xml.Functions.Core;
using NUnit.Framework;

namespace Agenix.Validation.Xml.Tests.Functions.Core;

[TestFixture]
public class XpathFunctionTest : AbstractNUnitSetUp
{
    private readonly XpathFunction _function = new();
    private const string XmlSource = "<person><name>Sheldon</name><age>29</age></person>";

    [Test]
    public void TestExecuteXpath()
    {
        var parameters = new List<string> { XmlSource, "/person/name" };

        Assert.That(_function.Execute(parameters, Context), Is.EqualTo("Sheldon"));
    }

    [Test]
    public void TestExecuteXpathWithNamespaces()
    {
        var parameters = new List<string>();
        const string xmlSourceNamespace =
            "<person xmlns=\"http://agenix.sample.org/person\"><name>Sheldon</name><age>29</age></person>";
        parameters.Add(xmlSourceNamespace);
        parameters.Add("/p:person/p:name");

        Context.NamespaceContextBuilder.NamespaceMappings["p"] = "http://agenix.sample.org/person";

        Assert.That(_function.Execute(parameters, Context), Is.EqualTo("Sheldon"));
    }

    [Test]
    public void TestExecuteXpathUnknown()
    {
        var parameters = new List<string> { XmlSource, "/person/unknown" };

        Assert.That(() => _function.Execute(parameters, Context),
            Throws.TypeOf<AgenixSystemException>());
    }

    [Test]
    public void ShouldLookupFunction()
    {
        var functionLookup = IFunction.Lookup();

        Assert.That(functionLookup.ContainsKey("xpath"), Is.True);
        Assert.That(functionLookup["xpath"].GetType(), Is.EqualTo(typeof(XpathFunction)));

        var defaultLibrary = new DefaultFunctionLibrary();
        Assert.That(defaultLibrary.GetFunction("xpath").GetType(), Is.EqualTo(typeof(XpathFunction)));
    }
}
