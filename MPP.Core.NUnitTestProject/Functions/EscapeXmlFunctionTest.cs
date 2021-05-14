using System.Collections.Generic;
using MPP.Core.Exceptions;
using MPP.Core.Functions.Core;
using NUnit.Framework;

namespace MPP.Core.NUnitTestProject.Functions
{
    public class EscapeXmlFunctionTest : AbstractNUnitSetUp
    {
        private const string EscapedXml = "&lt;foo&gt;&lt;bar&gt;Yes, I like W0rld!&lt;/bar&gt;&lt;/foo&gt;";

        private const string XmlFragment = "<foo><bar>Yes, I like W0rld!</bar></foo>";
        private readonly EscapeXmlFunction _function = new();

        [Test]
        public void TestEscapeXmlFunction()
        {
            Assert.AreEqual(_function.Execute(new List<string> {XmlFragment}, Context), EscapedXml);
        }

        [Test]
        public void TestNoParameters()
        {
            Assert.Throws<InvalidFunctionUsageException>(() =>
                _function.Execute(new List<string>(), Context)
            );
        }
    }
}