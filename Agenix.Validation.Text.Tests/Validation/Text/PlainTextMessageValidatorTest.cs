using Agenix.Api.Exceptions;
using Agenix.Api.Message;
using Agenix.Api.Validation.Context;
using Agenix.Core.Message;
using Agenix.Validation.Text.Validation.Text;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Validation.Text.Tests.Validation.Text;

public class PlainTextMessageValidatorTest : AbstractNUnitSetUp
{
    private readonly IValidationContext _validationContext = new DefaultValidationContext();
    private PlainTextMessageValidator _validator;

    [SetUp]
    public new void Setup()
    {
        _validator = new PlainTextMessageValidator();
    }

    [Test]
    public void TestPlainTextValidation()
    {
        IMessage receivedMessage = new DefaultMessage("Hello World!");
        IMessage controlMessage = new DefaultMessage("Hello World!");

        _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);
    }

    [Test]
    public void TestPlainTextValidationWithIgnore()
    {
        IMessage receivedMessage = new DefaultMessage(
            $"Hello World, time is {DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}!");
        IMessage controlMessage = new DefaultMessage("Hello World, time is @Ignore@!");

        _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);

        controlMessage = new DefaultMessage("Hello @Ignore@, time is @Ignore@!");
        _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);

        controlMessage = new DefaultMessage("Hello @Ignore@, time is @Ignore@!");
        _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);

        controlMessage = new DefaultMessage("Hello @Ignore@, time is @Ignore(100)@");
        _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);

        controlMessage = new DefaultMessage("@Ignore(11)@, time is @Ignore@!");
        _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);

        controlMessage = new DefaultMessage("@Ignore@");
        _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);

        receivedMessage = new DefaultMessage(Guid.NewGuid().ToString());
        controlMessage = new DefaultMessage("@Ignore@-@Ignore@-@Ignore@-@Ignore@-@Ignore@");
        _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);

        receivedMessage = new DefaultMessage("1a2b3c4d_5e6f7g8h");
        controlMessage = new DefaultMessage("1a@Ignore(4)@4d_@Ignore(6)@8h");
        _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);

        receivedMessage = new DefaultMessage("Your id is 1a2b3c4d_5e6f7g8h");
        controlMessage = new DefaultMessage("Your id is @Ignore@");
        _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);
    }

    [Test]
    public void TestPlainTextValidationCreateVariable()
    {
        var time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        IMessage receivedMessage = new DefaultMessage($"Hello World, time is {time}!");
        IMessage controlMessage = new DefaultMessage("Hello World, time is @Variable(time)@!");

        _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);

        ClassicAssert.AreEqual(Context.GetVariable("time"), time.ToString());

        controlMessage = new DefaultMessage("Hello @Variable('world')@, time is @Variable(time)@!");
        _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);

        ClassicAssert.AreEqual(Context.GetVariable("world"), "World");
        ClassicAssert.AreEqual(Context.GetVariable("time"), time.ToString());

        var id = Guid.NewGuid().ToString();
        receivedMessage = new DefaultMessage(id);
        controlMessage = new DefaultMessage("@Variable('id')@");
        _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);

        ClassicAssert.AreEqual(Context.GetVariable("id"), id);

        receivedMessage = new DefaultMessage("Today is 24.12.2017");
        controlMessage = new DefaultMessage("Today is @Variable('date')@");
        _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);

        ClassicAssert.AreEqual(Context.GetVariable("date"), "24.12.2017");

        receivedMessage = new DefaultMessage("Today is 2017-12-24");
        controlMessage = new DefaultMessage("Today is @Variable('date')@");
        _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);

        ClassicAssert.AreEqual(Context.GetVariable("date"), "2017-12-24");
    }

    [Test]
    public void TestPlainTextValidationWithIgnoreFail()
    {
        var receivedMessage = new DefaultMessage("Hello World!");
        var controlMessage = new DefaultMessage("Hello @Ignore@");

        var exception = Assert.Throws<ValidationException>(() =>
        {
            _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);
        });

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message.Contains("only whitespaces!"), Is.False);
        Assert.That(exception.Message.Contains("expected 'Hello World'"), Is.True);
        Assert.That(exception.Message.Contains("but was 'Hello World!'"), Is.True);
    }

    [Test]
    public void TestPlainTextValidationContains()
    {
        var receivedMessage = new DefaultMessage("Hello World!");
        var controlMessage = new DefaultMessage("@Contains('World!')@");

        _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);
    }

    [Test]
    public void TestPlainTextValidationContainsError()
    {
        var receivedMessage = new DefaultMessage("Hello World!");
        var controlMessage = new DefaultMessage("@Contains('Space!')@");

        Assert.Throws<ValidationException>(() =>
        {
            _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);
        });
    }

    [Test]
    public void TestPlainTextValidationVariableSupport()
    {
        var receivedMessage = new DefaultMessage("Hello World!");
        var controlMessage = new DefaultMessage("Hello ${world}!");

        Context.SetVariable("world", "World");

        _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);
    }

    [Test]
    public void TestPlainTextValidationWrongValue()
    {
        var receivedMessage = new DefaultMessage("Hello World!");
        var controlMessage = new DefaultMessage("Hello Agenix!");

        var exception = Assert.Throws<ValidationException>(() =>
        {
            _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);
        });

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message.Contains("only whitespaces!"), Is.False);
        Assert.That(exception.Message.Contains("expected 'Hello Agenix!'"), Is.True);
        Assert.That(exception.Message.Contains("but was 'Hello World!'"), Is.True);
    }

    [Test]
    public void TestPlainTextValidationLeadingTrailingWhitespace()
    {
        var receivedMessage = new DefaultMessage("   Hello World!   ");
        var controlMessage = new DefaultMessage("Hello World!");

        _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);
    }

    [Test]
    public void TestPlainTextValidationMultiline()
    {
        var receivedMessage = new DefaultMessage("Hello\nWorld!\n");
        var controlMessage = new DefaultMessage("Hello\nWorld!\n");

        _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);
    }

    [Test]
    public void TestPlainTextValidationNormalizeWhitespaces()
    {
        var receivedMessage = new DefaultMessage(" Hello\r\n\n  \t World!\t\t\n\n    ");
        var controlMessage = new DefaultMessage("Hello\n World!\n");

        var exception = Assert.Throws<ValidationException>(() =>
        {
            _validator.IgnoreNewLineType = true;
            _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);
        });

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message.Contains("only whitespaces!"), Is.True);
        _validator.IgnoreWhitespace = true;
        _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);
    }

    [Test]
    public void TestPlainTextValidationNormalizeNewLineTypeCrlf()
    {
        var receivedMessage = new DefaultMessage("Hello\nWorld!\n");
        var controlMessage = new DefaultMessage("Hello\r\nWorld!\r\n");

        var exception = Assert.Throws<ValidationException>(() =>
        {
            _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);
        });

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message.Contains("only whitespaces!"), Is.True);
        _validator.IgnoreNewLineType = true;
        _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);
    }

    [Test]
    public void TestPlainTextValidationNormalizeNewLineTypeCr()
    {
        var receivedMessage = new DefaultMessage("Hello\nWorld!\n");
        var controlMessage = new DefaultMessage("Hello\rWorld!\r");

        var exception = Assert.Throws<ValidationException>(() =>
        {
            _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);
        });

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message.Contains("only whitespaces!"), Is.True);
        _validator.IgnoreNewLineType = true;
        _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);
    }
}