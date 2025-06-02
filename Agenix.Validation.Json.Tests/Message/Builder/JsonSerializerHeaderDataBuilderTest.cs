using Agenix.Api.Spi;
using Agenix.Validation.Json.Message.Builder;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Validation.Json.Tests.Message.Builder;

public class JsonSerializerHeaderDataBuilderTest : AbstractNUnitSetUp
{
    private readonly JsonSerializer _mapper = new();
    private readonly TestRequest _request = new("Hello Agenix!");
    private Mock<IReferenceResolver> _mockReferenceResolver;

    [SetUp]
    public void SetUp()
    {
        _mockReferenceResolver = new Mock<IReferenceResolver>();
        Context.SetReferenceResolver(_mockReferenceResolver.Object);
    }

    [Test]
    public void ShouldBuildHeaderData()
    {
        // Arrange
        _mockReferenceResolver.Setup(resolver => resolver.ResolveAll<JsonSerializer>())
            .Returns(new Dictionary<string, JsonSerializer> { { "mapper", _mapper } });
        _mockReferenceResolver.Setup(resolver => resolver.Resolve<JsonSerializer>())
            .Returns(_mapper);

        var builder = new JsonSerializerHeaderDataBuilder(_request);

        // Act
        var result = builder.BuildHeaderData(Context);

        // Assert
        ClassicAssert.AreEqual("{\"Message\":\"Hello Agenix!\"}", result);
    }

    [Test]
    public void ShouldBuildHeaderDataWithMapper()
    {
        var builder = new JsonSerializerHeaderDataBuilder(_request, _mapper);

        ClassicAssert.AreEqual(builder.BuildHeaderData(Context), "{\"Message\":\"Hello Agenix!\"}");
    }

    [Test]
    public void ShouldBuildHeaderDataWithMapperName()
    {
        // Arrange
        _mockReferenceResolver.Setup(resolver => resolver.IsResolvable("mapper")).Returns(true);
        _mockReferenceResolver.Setup(resolver => resolver.Resolve<JsonSerializer>("mapper")).Returns(_mapper);

        Context.SetReferenceResolver(_mockReferenceResolver.Object);

        var builder = new JsonSerializerHeaderDataBuilder(_request, "mapper");

        // Act
        var result = builder.BuildHeaderData(Context);

        // Assert
        ClassicAssert.AreEqual("{\"Message\":\"Hello Agenix!\"}", result);
    }

    [Test]
    public void ShouldBuildHeaderDataWithVariableSupport()
    {
        Context.SetVariable("message", "Hello Agenix!");
        var builder = new JsonSerializerHeaderDataBuilder(new TestRequest("${message}"), _mapper);

        ClassicAssert.AreEqual(builder.BuildHeaderData(Context), "{\"Message\":\"Hello Agenix!\"}");
    }
}
