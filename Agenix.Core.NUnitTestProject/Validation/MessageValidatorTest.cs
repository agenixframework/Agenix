using Agenix.Core.Validation;
using Agenix.Core.Validation.Context;
using Agenix.Core.Validation.Json;
using Agenix.Core.Validation.Text;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Core.NUnitTestProject.Validation;

/// <summary>
///     Unit tests for the <see cref="MessageValidator" /> class.
/// </summary>
public class MessageValidatorTest
{
    [Test]
    public void TestLookup()
    {
        var validators = IMessageValidator<IValidationContext>.Lookup();
        ClassicAssert.AreEqual(6, validators.Count);
        ClassicAssert.IsNotNull(validators["defaultValidator"]);
        ClassicAssert.AreEqual(validators["defaultValidator"].GetType(), typeof(DefaultMessageHeaderValidator));
        ClassicAssert.IsNotNull(validators["defaultPlaintextMessageValidator"]);
        ClassicAssert.AreEqual(validators["defaultPlaintextMessageValidator"].GetType(),
            typeof(PlainTextMessageValidator));
        ClassicAssert.IsNotNull(validators["defaultBinaryBase64MessageValidator"]);
        ClassicAssert.AreEqual(validators["defaultBinaryBase64MessageValidator"].GetType(),
            typeof(BinaryBase64MessageValidator));
        ClassicAssert.IsNotNull(validators["defaultGzipBinaryBase64MessageValidator"]);
        ClassicAssert.AreEqual(validators["defaultGzipBinaryBase64MessageValidator"].GetType(),
            typeof(GzipBinaryBase64MessageValidator));
        ClassicAssert.IsNotNull(validators["defaultJsonMessageValidator"]);
        ClassicAssert.AreEqual(validators["defaultJsonMessageValidator"].GetType(),
            typeof(JsonTextMessageValidator));
        ClassicAssert.IsNotNull(validators["defaultJsonPathMessageValidator"]);
        ClassicAssert.AreEqual(validators["defaultJsonPathMessageValidator"].GetType(),
            typeof(JsonPathMessageValidator));
    }

    [Test]
    public void TestTestLookup()
    {
        ClassicAssert.IsTrue(IMessageValidator<IValidationContext>.Lookup("header").IsPresent);
        ClassicAssert.IsTrue(IMessageValidator<IValidationContext>.Lookup("plaintext").IsPresent);
        ClassicAssert.IsTrue(IMessageValidator<IValidationContext>.Lookup("binary_base64").IsPresent);
        ClassicAssert.IsTrue(IMessageValidator<IValidationContext>.Lookup("gzip_base64").IsPresent);
        ClassicAssert.IsTrue(IMessageValidator<IValidationContext>.Lookup("json").IsPresent);
        ClassicAssert.IsTrue(IMessageValidator<IValidationContext>.Lookup("json-path").IsPresent);
    }
}