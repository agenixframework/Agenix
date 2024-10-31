using Agenix.Core.Exceptions;
using Agenix.Core.Validation.Json.Dsl;
using NUnit.Framework;

namespace Agenix.Core.NUnitTestProject.Validation.Json;

public class JsonPathValidatorTest : AbstractNUnitSetUp
{
    private readonly string _payload = "{ \"root\": {"
                                       + "\"element\": { \"attributeA\":\"attribute-value\",\"attributeB\":\"attribute-value\",\"sub-element\":\"text-value\" },"
                                       + "\"text\": \"text-value\","
                                       + "\"nullValue\": null,"
                                       + "\"number\": 10,"
                                       + "\"numbers\": [10, 20, 30, 40],"
                                       + "\"person\": {\"name\": \"Penny\"},"
                                       + "\"nerds\": [ {\"name\": \"Leonard\"}, {\"name\": \"Sheldon\"} ]"
                                       + "}}";


    [Test]
    public void TestValidateMessageElementsWithJsonPathSuccessful()
    {
        Context.SetVariable("personName", "Penny");

        JsonSupport.Json().JsonPath().Validate()
            .Expression("$..element.sub-element", "text-value")
            .Expression("$['root']['element']['sub-element']", "text-value")
            .Expression("$..['sub-element']", "text-value")
            .Expression("$..sub-element", "text-value")
            .Expression("$.root.numbers", "[10,20,30,40]")
            .Expression("$.root.person.name", "${personName}")
            .Build()
            .Validate(_payload, Context);
    }

    [Test]
    public void TestValidateMessageElementsWithJsonPathFunctionsSuccessful()
    {
        JsonSupport.Json().JsonPath().Validate()
            .Expression("$..element.KeySet()", "attributeA, attributeB, sub-element")
            .Expression("$['root']['person'].KeySet()", "name")
            .Expression("$.root.numbers.Size()", "4")
            .Expression("$.root.person.Size()", "1")
            .Expression("$.root.person.Exists()", "True")
            .Expression("$.root.nullValue", "")
            .Expression("$.root.nerds.Size()", "2")
            .Expression("$..sub-element.Size()", "1")
            .Expression("$.root.nerds.ToString()", "[{\"name\":\"Leonard\"},{\"name\":\"Sheldon\"}]")
            .Build()
            .Validate(_payload, Context);
    }

    [Test]
    public void TestValidateMessageElementsWithValidationMatcherSuccessful()
    {
        JsonSupport.Json().JsonPath().Validate()
            .Expression("$.root.element", "@Ignore()@")
            .Expression("$.root.person.name", "@EqualsIgnoreCase('PENNY')@")
            .Expression("$..element.sub-element", "@ContainsIgnoreCase('-VALUE')@")
            .Expression("$..sub-element", "@Contains('-value')@")
            .Expression("$..['sub-element']", "@EndsWith('-value')@")
            .Expression("$.root.number", "@LowerThan(11)@")
            .Expression("$..number", "@GreaterThan(9)@")
            .Expression("$..['number']", "@IsNumber()@")
            .Expression("$..text", "@StartsWith('text-')@")
            .Expression("$.root.text", "@StringLength(10)@")
            .Build()
            .Validate(_payload, Context);
    }

    [Test]
    public void TestValidateMessageElementsWithJsonPathFunctionsNotSuccessful()
    {
        try
        {
            // element does not exist
            JsonSupport.Json().JsonPath().Validate()
                .Expression("$.element.KeySet()", "attributeA, attributeB, attributeC")
                .Build()
                .Validate(_payload, Context);
            Assert.Fail("Missing validation exception");
        }
        catch (CoreSystemException)
        {
        }

        try
        {
            // element does not exist
            JsonSupport.Json().JsonPath().Validate()
                .Expression("$.root.numbers.Size()", "5")
                .Build()
                .Validate(_payload, Context);
            Assert.Fail("Missing validation exception");
        }
        catch (ValidationException)
        {
        }

        try
        {
            // element does not exist
            JsonSupport.Json().JsonPath().Validate()
                .Expression("$.root.person.Size()", "0")
                .Build()
                .Validate(_payload, Context);
            Assert.Fail("Missing validation exception");
        }
        catch (ValidationException)
        {
        }

        try
        {
            // element does not exist
            JsonSupport.Json().JsonPath().Validate()
                .Expression("$.root.nullValue", "10")
                .Build()
                .Validate(_payload, Context);
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
            JsonSupport.Json().JsonPath().Validate()
                .Expression("$..element.attributeA", "@StartsWith('attribute-')@")
                .Expression("$..element.attributeB", "@EndsWith('-value')@")
                .Expression("$..element.sub-element", "@Contains('FAIL')@")
                .Build()
                .Validate(_payload, Context)
        );
    }


    [Test]
    public void TestValidateMessageElementsWithJsonPathNotSuccessful()
    {
        Assert.Throws<ValidationException>(() =>
            JsonSupport.Json().JsonPath().Validate()
                .Expression("$..element.sub-element", "false-value")
                .Build()
                .Validate(_payload, Context)
        );
    }

    [Test]
    public void TestValidateMessageElementsWithFullPathSuccessful()
    {
        JsonSupport.Json().JsonPath().Validate()
            .Expression("$.root.element.sub-element", "text-value")
            .Build()
            .Validate(_payload, Context);
    }

    [Test]
    public void TestValidateMessageElementsWithFullPathNotSuccessful()
    {
        Assert.Throws<ValidationException>(() =>
            JsonSupport.Json().JsonPath().Validate()
                .Expression("$.root.element.sub-element", "false-value")
                .Build()
                .Validate(_payload, Context)
        );
    }

    [Test]
    public void TestValidateMessageElementsPathNotFound()
    {
        Assert.Throws<CoreSystemException>(() =>
            JsonSupport.Json().JsonPath().Validate()
                .Expression("$.root.foo", "foo-value")
                .Build()
                .Validate(_payload, Context)
        );
    }

    [Test]
    public void TestValidateMessageElementsWithMixedNotationsSuccessful()
    {
        //mix of xpath and dot-notation
        JsonSupport.Json().JsonPath().Validate()
            .Expression("$..element.sub-element", "text-value")
            .Expression("$.root.element.sub-element", "text-value")
            .Build()
            .Validate(_payload, Context);
    }
}