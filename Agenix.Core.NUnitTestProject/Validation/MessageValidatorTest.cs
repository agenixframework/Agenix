using Agenix.Api.Validation;
using Agenix.Api.Validation.Context;
using Agenix.Core.Validation;
using Agenix.Core.Validation.Json;
using Agenix.Validation.Json.Validation;
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
        ClassicAssert.AreEqual(3, validators.Count);
        ClassicAssert.IsNotNull(validators["header"]);
        ClassicAssert.AreEqual(validators["header"].GetType(), typeof(DefaultMessageHeaderValidator));
        ClassicAssert.IsNotNull(validators["json"]);
        ClassicAssert.AreEqual(validators["json"].GetType(),
            typeof(JsonTextMessageValidator));
        ClassicAssert.IsNotNull(validators["json-path"]);
        ClassicAssert.AreEqual(validators["json-path"].GetType(),
            typeof(JsonPathMessageValidator));
    }

    [Test]
    public void TestTestLookup()
    {
        ClassicAssert.IsTrue(IMessageValidator<IValidationContext>.Lookup("header").IsPresent);
    }
}