using System.Collections.Generic;
using Agenix.Core.Exceptions;
using Agenix.Core.Message;
using Agenix.Core.Validation.Json;
using Agenix.Core.Validation.Matcher;
using NHamcrest;
using NUnit.Framework;
using Has = NHamcrest.Has;
using Is = NHamcrest.Is;
using NUnitIs = NUnit.Framework.Is;


namespace Agenix.Core.NUnitTestProject.Validation.Json;

public class JsonPathMessageValidatorTest : AbstractNUnitSetUp
{
    private static readonly string Payload = "{ \"root\": {"
                                             + "\"element\": { \"attributeA\":\"attribute-value\",\"attributeB\":\"attribute-value\",\"sub-element\":\"text-value\" },"
                                             + "\"text\": \"text-value\","
                                             + "\"nullValue\": null,"
                                             + "\"number\": 10,"
                                             + "\"numbers\": [10, 20, 30, 40],"
                                             + "\"person\": {\"name\": \"Penny\"},"
                                             + "\"nerds\": [ {\"name\": \"Leonard\"}, {\"name\": \"Sheldon\"} ]"
                                             + "}}";

    private readonly IMessage _message = new DefaultMessage(Payload);
    private readonly JsonPathMessageValidator _validator = new();


    [Test]
    public void TestValidateMessageElementsWithJsonPathSuccessful()
    {
        var validationContext = new JsonPathMessageValidationContext.Builder()
            .Expression("$..element.sub-element", "text-value")
            .Build();
        _validator.ValidateMessage(_message, new DefaultMessage(), Context, validationContext);

        validationContext = new JsonPathMessageValidationContext.Builder()
            .Expression("$['root']['element']['sub-element']", "text-value")
            .Build();
        _validator.ValidateMessage(_message, new DefaultMessage(), Context, validationContext);

        validationContext = new JsonPathMessageValidationContext.Builder()
            .Expression("$..['sub-element']", "text-value")
            .Build();
        _validator.ValidateMessage(_message, new DefaultMessage(), Context, validationContext);

        validationContext = new JsonPathMessageValidationContext.Builder()
            .Expression("$..sub-element", "text-value")
            .Build();
        _validator.ValidateMessage(_message, new DefaultMessage(), Context, validationContext);

        validationContext = new JsonPathMessageValidationContext.Builder()
            .Expression("$.root.numbers", "[10,20,30,40]")
            .Build();

        _validator.ValidateMessage(_message, new DefaultMessage(), Context, validationContext);

        validationContext = new JsonPathMessageValidationContext.Builder()
            .Expression("$..sub-element", Starts.With("text"))
            .Build();

        _validator.ValidateMessage(_message, new DefaultMessage(), Context, validationContext);

        validationContext = new JsonPathMessageValidationContext.Builder()
            .Expression("$..name", Has.Item(Is.EqualTo("Penny")))
            .Build();
        _validator.ValidateMessage(_message, new DefaultMessage(), Context, validationContext);

        validationContext = new JsonPathMessageValidationContext.Builder()
            .Expression("$..name", Has.Items(Is.EqualTo("Leonard"), Is.EqualTo("Sheldon"), Is.EqualTo("Penny")))
            .Build();
        _validator.ValidateMessage(_message, new DefaultMessage(), Context, validationContext);
    }


    [Test]
    public void TestValidateMessageElementsWithJsonPathFunctionsSuccessful()
    {
        var validationContext = new JsonPathMessageValidationContext.Builder()
            .Expression("$..element.KeySet()", "attributeA, attributeB, sub-element")
            .Build();
        _validator.ValidateMessage(_message, new DefaultMessage(), Context, validationContext);

        validationContext = new JsonPathMessageValidationContext.Builder()
            .Expression("$.root.element.KeySet()", new List<string> { "attributeA", "sub-element", "attributeB" })
            .Build();
        _validator.ValidateMessage(_message, new DefaultMessage(), Context, validationContext);

        validationContext = new JsonPathMessageValidationContext.Builder()
            .Expression("$.root.element.KeySet()",
                Has.Items(Is.EqualTo("attributeA"), Is.EqualTo("sub-element"), Is.EqualTo("attributeB")))
            .Build();
        _validator.ValidateMessage(_message, new DefaultMessage(), Context, validationContext);

        validationContext = new JsonPathMessageValidationContext.Builder()
            .Expression("$['root']['person'].KeySet()", Is.EqualTo("name"))
            .Build();
        _validator.ValidateMessage(_message, new DefaultMessage(), Context, validationContext);

        validationContext = new JsonPathMessageValidationContext.Builder()
            .Expression("$.root.numbers.Size()", 4)
            .Build();
        _validator.ValidateMessage(_message, new DefaultMessage(), Context, validationContext);

        validationContext = new JsonPathMessageValidationContext.Builder()
            .Expression("$.root.person.Size()", 1)
            .Build();
        _validator.ValidateMessage(_message, new DefaultMessage(), Context, validationContext);

        validationContext = new JsonPathMessageValidationContext.Builder()
            .Expression("$.root.person.Exists()", true)
            .Build();
        _validator.ValidateMessage(_message, new DefaultMessage(), Context, validationContext);

        validationContext = new JsonPathMessageValidationContext.Builder()
            .Expression("$.root.nullValue", "")
            .Build();
        _validator.ValidateMessage(_message, new DefaultMessage(), Context, validationContext);

        validationContext = new JsonPathMessageValidationContext.Builder()
            .Expression("$.root.nerds.Size()", 2L)
            .Build();

        _validator.ValidateMessage(_message, new DefaultMessage(), Context, validationContext);

        validationContext = new JsonPathMessageValidationContext.Builder()
            .Expression("$..sub-element.Size()", 1L)
            .Build();
        _validator.ValidateMessage(_message, new DefaultMessage(), Context, validationContext);

        validationContext = new JsonPathMessageValidationContext.Builder()
            .Expression("$.root.nerds.ToString()", "[{\"name\":\"Leonard\"},{\"name\":\"Sheldon\"}]")
            .Build();
        _validator.ValidateMessage(_message, new DefaultMessage(), Context, validationContext);
    }

    [Test]
    public void TestValidateMessageElementsWithValidationMatcherSuccessful()
    {
        Context.ValidationMatcherRegistry.GetLibraryForPrefix("").Members.Add("Assert", new NullValueMatcher());
        var validationExpressions = new Dictionary<string, object>
        {
            { "$..element.attributeA", "@StartsWith('attribute-')@" },
            { "$..element.attributeB", "@EndsWith('-value')@" },
            { "$..element.sub-element", "@EqualsIgnoreCase('TEXT-VALUE')@" },
            { "$.root.element.sub-element", "@Contains('ext-val')@" },
            { "$.root.nullValue", "@Assert(NullValue())@" }
        };

        var validationContext = new JsonPathMessageValidationContext.Builder()
            .Expressions(validationExpressions)
            .Build();

        _validator.ValidateMessage(_message, new DefaultMessage(), Context, validationContext);
    }

    [Test]
    public void TestValidateMessageElementsWithJsonPathFunctionsNotSuccessful()
    {
        try
        {
            // element does not exist
            var validationContext = new JsonPathMessageValidationContext.Builder()
                .Expression("$.element.KeySet()", "attributeA, attributeB, attributeC")
                .Build();
            _validator.ValidateMessage(_message, new DefaultMessage(), Context, validationContext);
            Assert.Fail("Missing validation exception");
        }
        catch (CoreSystemException)
        {
        }

        try
        {
            // element does not exist
            var validationContext = new JsonPathMessageValidationContext.Builder()
                .Expression("$.element.KeySet()", new List<string> { "attributeA", "attributeB" })
                .Build();
            _validator.ValidateMessage(_message, new DefaultMessage(), Context, validationContext);
            Assert.Fail("Missing validation exception");
        }
        catch (CoreSystemException)
        {
        }

        try
        {
            // element does not exist
            var validationContext = new JsonPathMessageValidationContext.Builder()
                .Expression("$.root.numbers.Size()", "5")
                .Build();
            _validator.ValidateMessage(_message, new DefaultMessage(), Context, validationContext);
            Assert.Fail("Missing validation exception");
        }
        catch (ValidationException)
        {
        }

        try
        {
            // element does not exist
            var validationContext = new JsonPathMessageValidationContext.Builder()
                .Expression("$.root.person.Size()", "0")
                .Build();
            _validator.ValidateMessage(_message, new DefaultMessage(), Context, validationContext);
            Assert.Fail("Missing validation exception");
        }
        catch (ValidationException)
        {
        }

        try
        {
            // element does not exist
            var validationContext = new JsonPathMessageValidationContext.Builder()
                .Expression("$.root.nullValue", "10")
                .Build();
            _validator.ValidateMessage(_message, new DefaultMessage(), Context, validationContext);
            Assert.Fail("Missing validation exception");
        }
        catch (ValidationException)
        {
        }
    }

    [Test]
    public void TestValidateMessageElementsWithValidationMatcherNotSuccessful()
    {
        Assert.Throws<ValidationException>(() =>
            {
                var validationContext = new JsonPathMessageValidationContext.Builder()
                    .Expression("$..element.attributeA", "@StartsWith('attribute-')@")
                    .Expression("$..element.attributeB", "@EndsWith('-value')@")
                    .Expression("$..element.sub-element", "@Contains('FAIL')@")
                    .Build();
                _validator.ValidateMessage(_message, new DefaultMessage(), Context, validationContext);
            }
        );
    }


    [Test]
    public void TestValidateMessageElementsWithJsonPathNotSuccessful()
    {
        Assert.Throws<ValidationException>(() =>
            {
                var validationContext = new JsonPathMessageValidationContext.Builder()
                    .Expression("$..element.sub-element", "false-value")
                    .Build();
                _validator.ValidateMessage(_message, new DefaultMessage(), Context, validationContext);
            }
        );
    }


    [Test]
    public void TestValidateMessageElementsWithFullPathSuccessful()
    {
        var validationContext = new JsonPathMessageValidationContext.Builder()
            .Expression("$.root.element.sub-element", "text-value")
            .Build();
        _validator.ValidateMessage(_message, new DefaultMessage(), Context, validationContext);
    }

    [Test]
    public void TestValidateMessageElementsWithFullPathNotSuccessful()
    {
        Assert.Throws<ValidationException>(() =>
            {
                var validationContext = new JsonPathMessageValidationContext.Builder()
                    .Expression("$.root.element.sub-element", "false-value")
                    .Build();
                _validator.ValidateMessage(_message, new DefaultMessage(), Context, validationContext);
            }
        );
    }

    [Test]
    public void TestValidateMessageElementsPathNotFound()
    {
        Assert.Throws<CoreSystemException>(() =>
            {
                var validationContext = new JsonPathMessageValidationContext.Builder()
                    .Expression("$.root.foo", "foo-value")
                    .Build();
                _validator.ValidateMessage(_message, new DefaultMessage(), Context, validationContext);
            }
        );
    }

    [Test]
    public void TestValidateMessageElementsWithMixedNotationsSuccessful()
    {
        //mix of xpath and dot-notation
        var validationContext = new JsonPathMessageValidationContext.Builder()
            .Expression("$..element.sub-element", "text-value")
            .Expression("$.root.element.sub-element", "text-value")
            .Build();
        _validator.ValidateMessage(_message, new DefaultMessage(), Context, validationContext);
    }

    private sealed class NullValueMatcher : IValidationMatcher
    {
        public void Validate(string fieldName, string value, List<string> controlParameters, TestContext context)
        {
            switch (controlParameters[0])
            {
                case "NullValue()":
                    Assert.That(value, NUnitIs.Null);
                    break;
                case "NotNullValue()":
                    Assert.That(value, NUnitIs.Not.Null);
                    break;
            }
        }
    }
}