using Agenix.Api.Message;
using Agenix.Api.Spi;
using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Agenix.Core.Message;
using Agenix.Core.Util;
using Agenix.Core.Validation.Json;
using Agenix.Validation.Json.Json;
using Agenix.Validation.Json.Json.Schema;
using Agenix.Validation.Json.Validation.Schema;
using Moq;
using NUnit.Framework;

namespace Agenix.Validation.Json.Tests.Json.Schema;

public class JsonSchemaValidationTest
{
    private JsonSchemaValidation _fixture;
    private Mock<JsonSchemaFilter> _jsonSchemaFilterMock;
    private IDisposable _mocks;
    private Mock<IReferenceResolver> _referenceResolverMock;
    private Mock<JsonMessageValidationContext> _validationContextMock;

    [SetUp]
    public void BeforeMethodSetup()
    {
        _referenceResolverMock = new Mock<IReferenceResolver>();
        _validationContextMock = new Mock<JsonMessageValidationContext>();
        _jsonSchemaFilterMock = new Mock<JsonSchemaFilter>();
        _fixture = new JsonSchemaValidation(_jsonSchemaFilterMock.Object);
    }

    [TearDown]
    public void AfterMethod()
    {
        _mocks?.Dispose();
    }

    [Test]
    public void TestValidJsonMessageSuccessfullyValidated()
    {
        // Setup json schema repositories
        var jsonSchemaRepository = new JsonSchemaRepository();
        jsonSchemaRepository.SetName("schemaRepository1");
        var schemaResource = FileUtils.GetFileResource(
            "assembly://Agenix.Validation.Json.Tests/Agenix.Validation.Json.Tests.Resources.Validation/ProductsSchema.json");
        var schema = new SimpleJsonSchema(schemaResource);
        schema.Initialize();
        jsonSchemaRepository.Schemas.Add(schema);

        // Add json schema repositories to a list
        var schemaRepositories = new List<JsonSchemaRepository> { jsonSchemaRepository };

        // Fix: Mock the filter behavior with a specific setup instead of using It.IsAny
        _jsonSchemaFilterMock
            .Setup(x => x.Filter(schemaRepositories,
                _validationContextMock.Object,
                _referenceResolverMock.Object))
            .Returns([schema]);


        // Create the received message
        var receivedMessage = new DefaultMessage("""
                                                 [
                                                           {
                                                             "id": 2,
                                                             "name": "An ice sculpture",
                                                             "price": 12.50,
                                                             "tags": ["cold", "ice"],
                                                             "dimensions": {
                                                             "length": 7.0,
                                                             "width": 12.0,
                                                             "height": 9.5
                                                              }
                                                           }
                                                         ]
                                                 """);

        var report = _fixture.Validate(
            receivedMessage,
            schemaRepositories,
            _validationContextMock.Object,
            _referenceResolverMock.Object);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(report.IsSuccess, Is.True);
            Assert.That(report.ValidationErrors, Has.Count.EqualTo(0));
        }
    }

    [Test]
    public void TestInvalidJsonMessageValidationIsNotSuccessful()
    {
        // Setup json schema repositories
        var jsonSchemaRepository = new JsonSchemaRepository();
        jsonSchemaRepository.SetName("schemaRepository1");
        var schemaResource = FileUtils.GetFileResource(
            "assembly://Agenix.Validation.Json.Tests/Agenix.Validation.Json.Tests.Resources.Validation/ProductsSchema.json");
        var schema = new SimpleJsonSchema(schemaResource);
        schema.Initialize();
        jsonSchemaRepository.Schemas.Add(schema);

        // Add json schema repositories to a list
        var schemaRepositories = new List<JsonSchemaRepository> { jsonSchemaRepository };

        // Mock the filter behavior
        _jsonSchemaFilterMock
            .Setup(x => x.Filter(schemaRepositories,
                _validationContextMock.Object,
                _referenceResolverMock.Object))
            .Returns([schema]);

        // Create the received message with the missing required "id" field
        var receivedMessage = new DefaultMessage("""
                                                     [
                                                       {
                                                         "name": "An ice sculpture",
                                                         "price": 12.50,
                                                         "tags": ["cold", "ice"],
                                                         "dimensions": {
                                                         "length": 7.0,
                                                         "width": 12.0,
                                                         "height": 9.5
                                                          }
                                                       }
                                                     ]
                                                 """);

        var report = _fixture.Validate(
            receivedMessage,
            schemaRepositories,
            _validationContextMock.Object,
            _referenceResolverMock.Object);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(report.IsSuccess, Is.False);
            Assert.That(report.ValidationErrors, Has.Count.EqualTo(1));
        }
    }

    [Test]
    public void TestValidationIsSuccessfulIfOneSchemaMatches()
    {
        // Setup json schema repositories
        var jsonSchemaRepository = new JsonSchemaRepository();
        jsonSchemaRepository.SetName("schemaRepository1");

        var schemaResource = FileUtils.GetFileResource(
            "assembly://Agenix.Validation.Json.Tests/Agenix.Validation.Json.Tests.Resources.Validation/BookSchema.json");
        var bookSchema = new SimpleJsonSchema(schemaResource);
        bookSchema.Initialize();
        jsonSchemaRepository.Schemas.Add(bookSchema);

        schemaResource = FileUtils.GetFileResource(
            "assembly://Agenix.Validation.Json.Tests/Agenix.Validation.Json.Tests.Resources.Validation/ProductsSchema.json");
        var productsSchema = new SimpleJsonSchema(schemaResource);
        productsSchema.Initialize();
        jsonSchemaRepository.Schemas.Add(productsSchema);

        // Add json schema repositories to a list
        var schemaRepositories = new List<JsonSchemaRepository> { jsonSchemaRepository };

        // Mock the filter behavior
        _jsonSchemaFilterMock
            .Setup(x => x.Filter(schemaRepositories,
                _validationContextMock.Object,
                _referenceResolverMock.Object))
            .Returns([productsSchema]);

        // Create the received message
        var receivedMessage = new DefaultMessage("""
                                                     [
                                                       {
                                                         "id": 2,
                                                         "name": "An ice sculpture",
                                                         "price": 12.50,
                                                         "tags": ["cold", "ice"],
                                                         "dimensions": {
                                                         "length": 7.0,
                                                         "width": 12.0,
                                                         "height": 9.5
                                                          }
                                                       }
                                                     ]
                                                 """);

        var report = _fixture.Validate(
            receivedMessage,
            schemaRepositories,
            _validationContextMock.Object,
            _referenceResolverMock.Object);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(report.IsSuccess, Is.True);
            Assert.That(report.ValidationErrors, Has.Count.EqualTo(0));
        }
    }

    [Test]
    public void TestValidationIsSuccessfulIfOneSchemaMatchesWithRepositoryMerge()
    {
        var repositoryList = new List<JsonSchemaRepository>();

        // Setup Repository 1 - does not contain the valid schema
        var repository1 = new JsonSchemaRepository();
        repository1.SetName("schemaRepository1");

        var schemaResource = FileUtils.GetFileResource(
            "assembly://Agenix.Validation.Json.Tests/Agenix.Validation.Json.Tests.Resources.Validation/BookSchema.json");
        var invalidSchema = new SimpleJsonSchema(schemaResource);
        invalidSchema.Initialize();
        repository1.Schemas.Add(invalidSchema);
        repositoryList.Add(repository1);

        // Setup Repository 2 - contains the valid schema
        var repository2 = new JsonSchemaRepository();
        repository2.SetName("schemaRepository2");

        schemaResource = FileUtils.GetFileResource(
            "assembly://Agenix.Validation.Json.Tests/Agenix.Validation.Json.Tests.Resources.Validation/ProductsSchema.json");
        var validSchema = new SimpleJsonSchema(schemaResource);
        validSchema.Initialize();
        repository2.Schemas.Add(validSchema);
        repositoryList.Add(repository2);

        // Mock the filter behavior
        _jsonSchemaFilterMock
            .Setup(x => x.Filter(repositoryList,
                _validationContextMock.Object,
                _referenceResolverMock.Object))
            .Returns([invalidSchema, validSchema]);

        // Create the received message
        var receivedMessage = new DefaultMessage("""
                                                     [
                                                       {
                                                         "id": 2,
                                                         "name": "An ice sculpture",
                                                         "price": 12.50,
                                                         "tags": ["cold", "ice"],
                                                         "dimensions": {
                                                         "length": 7.0,
                                                         "width": 12.0,
                                                         "height": 9.5
                                                          }
                                                       }
                                                     ]
                                                 """);

        var report = _fixture.Validate(
            receivedMessage,
            repositoryList,
            _validationContextMock.Object,
            _referenceResolverMock.Object);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(report.IsSuccess, Is.True);
            Assert.That(report.ValidationErrors, Has.Count.EqualTo(1));
        }
    }

    [Test]
    public void TestJsonSchemaFilterIsCalled()
    {
        // Create a mock repository with the required constructor parameters
        var repositoryMock = new Mock<JsonSchemaRepository>();
        var repositoryList = new List<JsonSchemaRepository> { repositoryMock.Object };
    
        var message = Mock.Of<IMessage>();
        var jsonMessageValidationContext = Mock.Of<JsonMessageValidationContext>();
    
        // Set up the mock to return an empty list of schemas (not null)
        _jsonSchemaFilterMock
            .Setup(x => x.Filter(repositoryList, jsonMessageValidationContext, _referenceResolverMock.Object))
            .Returns([]);

        _fixture.Validate(message, repositoryList, jsonMessageValidationContext, _referenceResolverMock.Object);

        _jsonSchemaFilterMock.Verify(x =>
            x.Filter(repositoryList, jsonMessageValidationContext, _referenceResolverMock.Object));
    }

    [Test]
    public void TestLookup()
    {
        var validators = ISchemaValidator<IMessageValidationContext>.Lookup();

        using (Assert.EnterMultipleScope())
        {
            Assert.That(validators, Has.Count.EqualTo(1));
            Assert.That(validators["defaultJsonSchemaValidator"], Is.Not.Null);
            Assert.That(validators["defaultJsonSchemaValidator"].GetType(), Is.EqualTo(typeof(JsonSchemaValidation)));
        }
    }

    [Test]
    public void TestTestLookup()
    {
        var result = ISchemaValidator<IMessageValidationContext>.Lookup("json");

        Assert.That(result.IsPresent, Is.True);
    }
}