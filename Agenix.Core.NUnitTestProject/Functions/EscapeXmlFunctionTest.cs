using System.Collections.Generic;
using Agenix.Core.Exceptions;
using Agenix.Core.Functions.Core;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Functions;

public class EscapeXmlFunctionTest : AbstractNUnitSetUp
{
    private const string EscapedXml = "&lt;foo&gt;&lt;bar&gt;Yes, I like W0rld!&lt;/bar&gt;&lt;/foo&gt;";

    private const string XmlFragment = "<foo><bar>Yes, I like W0rld!</bar></foo>";
    private readonly EscapeXmlFunction _function = new();

    [Test]
    public void TestEscapeXmlFunction()
    {
        ClassicAssert.AreEqual(_function.Execute([XmlFragment], Context), EscapedXml);
    }

    [Test]
    public void TestNoParameters()
    {
        Assert.Throws<InvalidFunctionUsageException>(() =>
            _function.Execute(new List<string>(), Context)
        );
    }
}