using Agenix.Api.Message;
using Agenix.Core.Endpoint.Adapter.Mapping;
using Agenix.Core.Message;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Endpoint.Adapter.Mapping;

/// <summary>
///     Unit tests for the AbstractMappingKeyExtractor class.
/// </summary>
public class AbstractMappingKeyExtractorTest
{
    [Test]
    public void TestMappingKeyPrefixSuffix()
    {
        var mappingKeyExtractor = new CustomMappingKeyExtractor();

        mappingKeyExtractor.SetMappingKeyPrefix("pre_");
        ClassicAssert.AreEqual("pre_key", mappingKeyExtractor.ExtractMappingKey(new DefaultMessage("")));

        mappingKeyExtractor.SetMappingKeySuffix("_end");
        ClassicAssert.AreEqual("pre_key_end", mappingKeyExtractor.ExtractMappingKey(new DefaultMessage("")));

        mappingKeyExtractor.SetMappingKeyPrefix("");
        ClassicAssert.AreEqual("key_end", mappingKeyExtractor.ExtractMappingKey(new DefaultMessage("")));
    }

    private class CustomMappingKeyExtractor : AbstractMappingKeyExtractor
    {
        protected override string GetMappingKey(IMessage request)
        {
            return "key";
        }
    }
}
