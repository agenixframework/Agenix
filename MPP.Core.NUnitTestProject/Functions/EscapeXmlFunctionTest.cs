using System.Collections.Generic;
using FleetPay.Core.Exceptions;
using FleetPay.Core.Functions.Core;
using NUnit.Framework;

namespace FleetPay.Core.NUnitTestProject.Functions
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