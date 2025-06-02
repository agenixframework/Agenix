using Agenix.Api.Exceptions;
using Agenix.Core.Message;
using Agenix.Validation.Json.Endpoint.Adapter.Mapping;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Endpoint.Adapter.Mapping;

/// <summary>
///     Unit tests for the <see cref="JsonPayloadMappingKeyExtractor" /> class.
/// </summary>
public class JsonPayloadMappingKeyExtractorTest
{
    [Test]
    public void TestExtractMappingKey()
    {
        var extractor = new JsonPayloadMappingKeyExtractor();
        extractor.SetJsonPathExpression("$.person.name");

        ClassicAssert.AreEqual(extractor.ExtractMappingKey(new DefaultMessage(
            "{ \"person\": {\"name\": \"Penny\"} }")), "Penny");

        ClassicAssert.AreEqual(extractor.ExtractMappingKey(new DefaultMessage(
            "{ \"person\": {\"name\": \"Leonard\"} }")), "Leonard");
    }

    [Test]
    public void TestExtractMappingKeyWithoutJsonPathExpressionSet()
    {
        var extractor = new JsonPayloadMappingKeyExtractor();

        ClassicAssert.AreEqual(extractor.ExtractMappingKey(new DefaultMessage(
            "{ \"person\": {\"name\": \"Penny\"} }")), "person");

        ClassicAssert.AreEqual(extractor.ExtractMappingKey(new DefaultMessage(
            "{ \"animal\": {\"name\": \"Sheldon\"} }")), "animal");
    }

    [Test]
    public void TestRouteMessageWithBadJsonPathExpression()
    {
        var extractor = new JsonPayloadMappingKeyExtractor();

        extractor.SetJsonPathExpression("$.I_DO_NOT_EXIST");

        Assert.Throws<AgenixSystemException>(() => extractor.ExtractMappingKey(new DefaultMessage(
            "{ \"person\": {\"name\": \"Penny\"} }")));
    }
}
