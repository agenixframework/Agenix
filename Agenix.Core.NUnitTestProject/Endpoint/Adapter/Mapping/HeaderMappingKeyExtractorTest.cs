using Agenix.Core.Endpoint.Adapter.Mapping;
using Agenix.Core.Exceptions;
using Agenix.Core.Message;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Endpoint.Adapter.Mapping;

/// <summary>
///     Unit tests for the HeaderMappingKeyExtractor class.
/// </summary>
public class HeaderMappingKeyExtractorTest
{
    [Test]
    public void TestExtractMappingKey()
    {
        var extractor = new HeaderMappingKeyExtractor();
        extractor.SetHeaderName("Foo");

        ClassicAssert.AreEqual(extractor.ExtractMappingKey(new DefaultMessage("Foo")
            .SetHeader("Foo", "foo")
            .SetHeader("Bar", "bar")), "foo");
    }

    [Test]
    public void TestExtractMappingKeyWithoutHeaderNameSet()
    {
        var extractor = new HeaderMappingKeyExtractor();

        Assert.Throws<CoreSystemException>(() => extractor.ExtractMappingKey(new DefaultMessage("Foo")
            .SetHeader("Foo", "foo")
            .SetHeader("Bar", "bar")));
    }

    [Test]
    public void TestExtractMappingKeyWithUnknownHeaderName()
    {
        var extractor = new HeaderMappingKeyExtractor("UNKNOWN");

        Assert.Throws<CoreSystemException>(() => extractor.ExtractMappingKey(new DefaultMessage("Foo")
            .SetHeader("Foo", "foo")
            .SetHeader("Bar", "bar")));
    }
}