using Agenix.Api.Exceptions;
using Agenix.Api.Validation.Context;
using Agenix.Core.Message;
using Agenix.Core.Validation.Json;
using Agenix.Validation.Json.Validation;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using TestContext = Agenix.Api.Context.TestContext;

namespace Agenix.Validation.Json.Tests.Json;

public class JsonTextMessageValidatorTest : AbstractNUnitSetUp
{
    private JsonTextMessageValidator _fixture;
    private JsonMessageValidationContext _validationContext;

    [SetUp]
    public new void Setup()
    {
        _fixture = new JsonTextMessageValidator();
        _validationContext = new JsonMessageValidationContext();
    }

    [Test]
    public void TestJsonValidationVariableSupport()
    {
        var actualMessage = new DefaultMessage("{\"text\":\"Hello World!\", \"index\":5, \"id\":\"x123456789x\"}");
        var expectedMessage =
            new DefaultMessage("{\"text\":\"Hello ${world}!\", \"index\":${index}, \"id\":\"${id}\"}");

        Context.SetVariable("world", "World");
        Context.SetVariable("index", "5");
        Context.SetVariable("id", "x123456789x");

        Assert.DoesNotThrow(() =>
            _fixture.ValidateMessage(actualMessage, expectedMessage, Context, _validationContext));
    }

    [Test]
    public void TestJsonValidationInvalidJsonText()
    {
        var actualMessage = new DefaultMessage("{\"text\":\"Hello World!\", \"index\":5, \"id\":\"wrong\"}");
        var expectedMessage =
            new DefaultMessage("{\"text\":\"Hello World!\", \"index\":invalid, \"id\":\"x123456789x\"}");

        var exception = Assert.Throws<AgenixSystemException>(() =>
            _fixture.ValidateMessage(actualMessage, expectedMessage, Context, _validationContext));

        ClassicAssert.IsNotNull(exception);
        ClassicAssert.IsNotNull(exception.InnerException);
        ClassicAssert.IsInstanceOf<JsonReaderException>(exception.InnerException);
    }

    [Test]
    public void TestJsonEmptyMessageValidationError()
    {
        var actualMessage = new DefaultMessage("{\"text\":\"Hello World!\", \"index\":5, \"id\":\"x123456789x\"}");
        var expectedMessage = new DefaultMessage("");

        Assert.DoesNotThrow(() =>
            _fixture.ValidateMessage(actualMessage, expectedMessage, Context, _validationContext));
    }

    [Test]
    public void ShouldUseCustomElementValidator()
    {
        // Arrange
        var message = new DefaultMessage("{}");

        var mockValidator = new Mock<JsonElementValidator>(false, Context, new HashSet<string>());
        var mockValidatorProvider = new Mock<IProvider>();

        mockValidatorProvider
            .Setup(p => p.GetValidator(It.IsAny<bool>(), It.IsAny<TestContext>(),
                It.IsAny<JsonMessageValidationContext>()))
            .Returns(mockValidator.Object);

        _fixture.Strict(true)
            .ElementValidatorProvider(mockValidatorProvider.Object)
            .ValidateMessage(message, message, Context, _validationContext);

        // Assert
        mockValidatorProvider.Verify(p => p.GetValidator(true, Context, _validationContext));
        mockValidator.Verify(v => v.Validate(It.IsAny<JsonElementValidatorItem<object>>()));
    }

    [Test]
    public void ShouldFindProperValidationContext()
    {
        var validationContexts = new List<IValidationContext>();
        validationContexts.Add(new HeaderValidationContext());
        validationContexts.Add(new JsonPathMessageValidationContext());

        Assert.That(_fixture.FindValidationContext(validationContexts), Is.Null);

        //validationContexts.Add(new XmlMessageValidationContext());

        //Assert.That(_fixture.FindValidationContext(validationContexts), Is.InstanceOf<XmlMessageValidationContext>());

        validationContexts.Add(new DefaultMessageValidationContext());

        Assert.That(_fixture.FindValidationContext(validationContexts),
            Is.InstanceOf<DefaultMessageValidationContext>());

        validationContexts.Add(new JsonMessageValidationContext());

        Assert.That(_fixture.FindValidationContext(validationContexts), Is.InstanceOf<JsonMessageValidationContext>());
    }
}
