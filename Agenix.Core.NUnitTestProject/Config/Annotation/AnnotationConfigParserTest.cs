using System;
using Agenix.Core.Config.Annotation;
using Agenix.Core.Endpoint;
using Agenix.Core.Endpoint.Direct.Annotation;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Config.Annotation;

/// <summary>
///     Unit tests for the AnnotationConfigParser implementations.
/// </summary>
public class AnnotationConfigParserTest
{
    [Test]
    public void TestLookup()
    {
        var parsers = IAnnotationConfigParser<Attribute, IEndpoint>.Lookup();
        ClassicAssert.AreEqual(parsers.Count, 2L);
        ClassicAssert.IsTrue(parsers.ContainsKey("direct.sync"));
        ClassicAssert.IsTrue(parsers.ContainsKey("direct.async"));
    }

    [Test]
    public void ShouldLookupParser()
    {
        ClassicAssert.IsTrue(IAnnotationConfigParser<Attribute, IEndpoint>.Lookup("direct.sync").IsPresent);
        ClassicAssert.AreEqual(IAnnotationConfigParser<Attribute, IEndpoint>.Lookup("direct.sync").Value.GetType(),
            typeof(DirectSyncEndpointConfigParser));
        ClassicAssert.IsTrue(IAnnotationConfigParser<Attribute, IEndpoint>.Lookup("direct.async").IsPresent);
        ClassicAssert.AreEqual(IAnnotationConfigParser<Attribute, IEndpoint>.Lookup("direct.async").Value.GetType(),
            typeof(DirectEndpointConfigParser));
    }
}