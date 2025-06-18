using Agenix.Api.Exceptions;
using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Agenix.Core.Message;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.Tests.Validation;

/// <summary>
///     Test class for validating the <see cref="DefaultTextEqualsMessageValidator" /> functionality.
/// </summary>
public class DefaultTextEqualsMessageValidatorTest : AbstractNUnitSetUp
{
    private readonly DefaultValidationContext _validationContext = new();
    private readonly DefaultTextEqualsMessageValidator _validator = new();

    [Test]
    [TestCaseSource(nameof(SuccessTests))]
    public void TestValidate(object received, object control)
    {
        var receivedMessage = new DefaultMessage(received);
        var controlMessage = new DefaultMessage(control);

        _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext);
    }

    [Test]
    [TestCaseSource(nameof(ErrorTests))]
    public void TestValidateError(object received, object control)
    {
        var receivedMessage = new DefaultMessage(received);
        var controlMessage = new DefaultMessage(control);

        Assert.Throws<ValidationException>(() =>
            _validator.ValidateMessage(receivedMessage, controlMessage, Context, _validationContext));
    }

    [Test]
    public void TestFirstDiff()
    {
        ClassicAssert.AreEqual(_validator.GetFirstDiff("Hello", "Hello"), "");
        ClassicAssert.AreEqual(_validator.GetFirstDiff("Hello", "Hi"), "at position 2 expected 'i', but was 'ello'");
        ClassicAssert.AreEqual(_validator.GetFirstDiff("Hello bar", "Hello foo"),
            "at position 7 expected 'foo', but was 'bar'");
        ClassicAssert.AreEqual(
            _validator.GetFirstDiff("Hello foo, how are you doing!", "Hello foo, how are you doing?"),
            "at position 29 expected '?', but was '!'");
        ClassicAssert.AreEqual(_validator.GetFirstDiff("Hello foo, how are you doing!", "Hello foo, how are you doing"),
            "at position 29 expected '', but was '!'");
        ClassicAssert.AreEqual(_validator.GetFirstDiff("Hello foo, how are you doing", "Hello foo, how are you doing!"),
            "at position 29 expected '!', but was ''");
        ClassicAssert.AreEqual(_validator.GetFirstDiff("1", "2"), "at position 1 expected '2', but was '1'");
        ClassicAssert.AreEqual(_validator.GetFirstDiff("1234", "1243"), "at position 3 expected '43', but was '34'");
        ClassicAssert.AreEqual(_validator.GetFirstDiff("nospacesatall", "no spaces at all"),
            "at position 3 expected ' spaces at all', but was 'spacesatall'");
    }

    /// <summary>
    ///     Provides a series of test cases for validating text equality scenarios using the DefaultTextEqualsMessageValidator
    ///     class.
    /// </summary>
    /// <returns>
    ///     A two-dimensional object array containing test case data where each test case consists of a pair of objects to be
    ///     compared for equality.
    /// </returns>
    private static object[][] SuccessTests()
    {
        return new object[][]
        {
            [null, null], ["", null], [null, ""], ["Hello World!", "Hello World!"], ["Hello World!  ", "Hello World!"],
            ["Hello World!", "Hello World!  "], ["Hello World!\n", "Hello World!"],
            ["Hello World!\n", "Hello World!\n"], ["\nHello World!", "\nHello World!"],
            ["Hello\nWorld!\n", "Hello\nWorld!\n"], ["Hello\r\nWorld!\r\n", "Hello\nWorld!\n"],
            ["Hello World!", null], // empty control message
            ["Hello World!", ""], // no control message
            ["Hello World!"u8.ToArray(), ""] // no control message
        };
    }

    /// <summary>
    ///     Provides a series of test cases for validating text inequality scenarios using the
    ///     DefaultTextEqualsMessageValidator class.
    /// </summary>
    /// <returns>
    ///     A two-dimensional object array containing test case data where each test case consists of a pair of objects
    ///     expected to be unequal.
    /// </returns>
    private static object[][] ErrorTests()
    {
        return new object[][]
        {
            [null, "Hello World!"], ["", "Hello World!"], ["Hello  World!", "Hello World!"],
            ["Hello World!", "Hello  World!"], ["Hello\nWorld!", "Hello World!"], ["Hello World!", "Hello\nWorld!"],
            ["Hello!", "Hi!"]
        };
    }
}
