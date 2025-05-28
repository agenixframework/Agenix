using Agenix.Api.Exceptions;
using Agenix.Api.Spi;
using Agenix.Core.Validation.Json;
using Agenix.Validation.Json.Json;
using Agenix.Validation.Json.Json.Schema;
using Agenix.Validation.Json.Validation.Schema;
using Moq;
using NUnit.Framework;

namespace Agenix.Validation.Json.Tests.Json.Schema;

public class JsonSchemaFilterTest
{
    private readonly JsonSchemaFilter _jsonSchemaFilter = new JsonSchemaFilter();
    private Mock<IReferenceResolver> _referenceResolverMock;

    [SetUp]
    public void SetupMocks()
    {
        _referenceResolverMock = new Mock<IReferenceResolver>();
    }
    
    [Test]
    public void TestFilterOnSchemaRepositoryName()
    {
        // GIVEN
        // Setup Schema repositories
        var firstJsonSchemaRepository = new JsonSchemaRepository();
        firstJsonSchemaRepository.SetName("schemaRepository1");
        var firstSimpleJsonSchema = Mock.Of<SimpleJsonSchema>();
        firstJsonSchemaRepository.Schemas.Add(firstSimpleJsonSchema);

        var secondJsonSchemaRepository = new JsonSchemaRepository();
        secondJsonSchemaRepository.SetName("schemaRepository2");
        var secondSimpleJsonSchema = Mock.Of<SimpleJsonSchema>();
        secondJsonSchemaRepository.Schemas.Add(secondSimpleJsonSchema);
        var thirdSimpleJsonSchema = Mock.Of<SimpleJsonSchema>();
        secondJsonSchemaRepository.Schemas.Add(thirdSimpleJsonSchema);

        var schemaRepositories = new List<JsonSchemaRepository> 
        { 
            firstJsonSchemaRepository, 
            secondJsonSchemaRepository 
        };

        // Setup validation context
        var validationContext = new JsonMessageValidationContext.Builder()
            .SchemaValidation(true)
            .SchemaRepository("schemaRepository2")
            .Build();

        // WHEN
        var simpleJsonSchemas = _jsonSchemaFilter.Filter(
            schemaRepositories, 
            validationContext, 
            _referenceResolverMock.Object);

        // THEN
        Assert.That(simpleJsonSchemas, Has.Count.EqualTo(2));
        Assert.That(simpleJsonSchemas, Does.Contain(secondSimpleJsonSchema));
        Assert.That(simpleJsonSchemas, Does.Contain(thirdSimpleJsonSchema));
    }
    
    [Test]
    public void TestFilterOnSchemaNameUsesApplicationContext()
    {
        // GIVEN
        // Setup Schema repositories
        var jsonSchemaRepository = new JsonSchemaRepository();
        jsonSchemaRepository.SetName("schemaRepository");
        var jsonSchema = Mock.Of<SimpleJsonSchema>();
        jsonSchemaRepository.Schemas.Add(jsonSchema);

        var schemaRepositories = new List<JsonSchemaRepository> { jsonSchemaRepository };

        // Setup validation context
        var validationContext = new JsonMessageValidationContext.Builder()
            .SchemaValidation(true)
            .Schema("mySchema")
            .Build();

        // Setup application context
        _referenceResolverMock
            .Setup(x => x.Resolve<SimpleJsonSchema>("mySchema"))
            .Returns(Mock.Of<SimpleJsonSchema>());

        // WHEN
        _jsonSchemaFilter.Filter(schemaRepositories, validationContext, _referenceResolverMock.Object);

        // THEN
        _referenceResolverMock.Verify(
            x => x.Resolve<object>(validationContext.Schema), 
            Times.Once());
    }
    
    [Test]
    public void TestFilterOnSchemaNameReturnsCorrectSchema()
    {
        // GIVEN
        // Setup Schema repositories
        var jsonSchemaRepository = new JsonSchemaRepository();
        jsonSchemaRepository.SetName("schemaRepository");
        var jsonSchema = Mock.Of<SimpleJsonSchema>();
        jsonSchemaRepository.Schemas.Add(jsonSchema);

        var schemaRepositories = new List<JsonSchemaRepository> { jsonSchemaRepository };

        // Setup validation context
        var validationContext = new JsonMessageValidationContext.Builder()
            .SchemaValidation(true)
            .Schema("mySchema")
            .Build();

        // Setup expected SimpleJsonSchema
        var expectedSimpleJsonSchema = Mock.Of<SimpleJsonSchema>();

        // Setup application context
        _referenceResolverMock
            .Setup(x => x.Resolve<SimpleJsonSchema>(validationContext.Schema))
            .Returns(expectedSimpleJsonSchema);

        // WHEN
        var simpleJsonSchemas = 
            _jsonSchemaFilter.Filter(schemaRepositories, validationContext, _referenceResolverMock.Object);

        // THEN
        Assert.That(simpleJsonSchemas, Has.Count.EqualTo(1));
        Assert.That(simpleJsonSchemas[0], Is.EqualTo(expectedSimpleJsonSchema));
    }
    
    [Test]
    public void TestNoSchemaRepositoryFoundThrowsException()
    {
        // GIVEN
        // Setup Schema repositories
        var firstJsonSchemaRepository = new JsonSchemaRepository();
        firstJsonSchemaRepository.SetName("schemaRepository1");
        var firstSimpleJsonSchema = Mock.Of<SimpleJsonSchema>();
        firstJsonSchemaRepository.Schemas.Add(firstSimpleJsonSchema);

        var schemaRepositories = new List<JsonSchemaRepository> { firstJsonSchemaRepository };

        // Setup validation context
        var validationContext = new JsonMessageValidationContext.Builder()
            .SchemaValidation(true)
            .SchemaRepository("schemaRepository2")
            .Build();

        // WHEN & THEN
        Assert.That(() => _jsonSchemaFilter.Filter(
                schemaRepositories, 
                validationContext, 
                _referenceResolverMock.Object),
            Throws.TypeOf<AgenixSystemException>());
    }
    
    [Test]
    public void TestNoSchemaFoundThrowsException()
    {
        // GIVEN
        // Setup Schema repositories
        var firstJsonSchemaRepository = new JsonSchemaRepository();
        firstJsonSchemaRepository.SetName("schemaRepository1");
        var firstSimpleJsonSchema = Mock.Of<SimpleJsonSchema>();
        firstJsonSchemaRepository.Schemas.Add(firstSimpleJsonSchema);

        var schemaRepositories = new List<JsonSchemaRepository> { firstJsonSchemaRepository };

        // Setup validation context
        var validationContext = new JsonMessageValidationContext.Builder()
            .SchemaValidation(true)
            .Schema("foo")
            .Build();

        // Setup application context
        _referenceResolverMock
            .Setup(x => x.Resolve<SimpleJsonSchema>(validationContext.Schema))
            .Throws<AgenixSystemException>();

        // WHEN & THEN
        Assert.That(() => _jsonSchemaFilter.Filter(
                schemaRepositories, 
                validationContext, 
                _referenceResolverMock.Object),
            Throws.TypeOf<AgenixSystemException>());
    }
    
    [Test]
    public void TestNoFilterReturnAllSchemas()
    {
        // GIVEN
        // Setup Schema repositories
        var firstJsonSchemaRepository = new JsonSchemaRepository();
        firstJsonSchemaRepository.SetName("schemaRepository1");
        var firstSimpleJsonSchema = Mock.Of<SimpleJsonSchema>();
        firstJsonSchemaRepository.Schemas.Add(firstSimpleJsonSchema);

        var secondJsonSchemaRepository = new JsonSchemaRepository();
        secondJsonSchemaRepository.SetName("schemaRepository2");
        var secondSimpleJsonSchema = Mock.Of<SimpleJsonSchema>();
        secondJsonSchemaRepository.Schemas.Add(secondSimpleJsonSchema);
        var thirdSimpleJsonSchema = Mock.Of<SimpleJsonSchema>();
        secondJsonSchemaRepository.Schemas.Add(thirdSimpleJsonSchema);

        var schemaRepositories = new List<JsonSchemaRepository> 
        { 
            firstJsonSchemaRepository, 
            secondJsonSchemaRepository 
        };

        // Setup validation context
        var validationContext = new JsonMessageValidationContext.Builder()
            .SchemaValidation(true)
            .Build();

        // WHEN
        var simpleJsonSchemas = _jsonSchemaFilter.Filter(
            schemaRepositories, 
            validationContext, 
            _referenceResolverMock.Object);

        // THEN
        Assert.That(simpleJsonSchemas, Has.Count.EqualTo(3));
        Assert.That(simpleJsonSchemas, Does.Contain(firstSimpleJsonSchema));
        Assert.That(simpleJsonSchemas, Does.Contain(secondSimpleJsonSchema));
        Assert.That(simpleJsonSchemas, Does.Contain(thirdSimpleJsonSchema));
    }

}