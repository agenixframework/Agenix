using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Agenix.Core.Validation;
using Agenix.Validation.Text.Validation.Text;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Validation.Text.Tests.Validation;

/// <summary>
///     Unit tests for the <see cref="MessageValidator" /> class.
/// </summary>
public class MessageValidatorTest
{
    [Test]
    public void TestLookup()
    {
        var validators = IMessageValidator<IValidationContext>.Lookup();
        ClassicAssert.AreEqual(4, validators.Count);
        ClassicAssert.IsNotNull(validators["header"]);
        ClassicAssert.AreEqual(validators["header"].GetType(), typeof(DefaultMessageHeaderValidator));
        ClassicAssert.IsNotNull(validators["plaintext"]);
        ClassicAssert.AreEqual(validators["plaintext"].GetType(),
            typeof(PlainTextMessageValidator));
        ClassicAssert.IsNotNull(validators["binary_base64"]);
        ClassicAssert.AreEqual(validators["binary_base64"].GetType(),
            typeof(BinaryBase64MessageValidator));
        ClassicAssert.IsNotNull(validators["gzip_base64"]);
        ClassicAssert.AreEqual(validators["gzip_base64"].GetType(),
            typeof(GzipBinaryBase64MessageValidator));
        ClassicAssert.IsNotNull(validators["gzip_base64"]);
    }

    [Test]
    public void TestTestLookup()
    {
        ClassicAssert.IsTrue(IMessageValidator<IValidationContext>.Lookup("header").IsPresent);
        ClassicAssert.IsTrue(IMessageValidator<IValidationContext>.Lookup("plaintext").IsPresent);
        ClassicAssert.IsTrue(IMessageValidator<IValidationContext>.Lookup("binary_base64").IsPresent);
        ClassicAssert.IsTrue(IMessageValidator<IValidationContext>.Lookup("gzip_base64").IsPresent);
    }
}
