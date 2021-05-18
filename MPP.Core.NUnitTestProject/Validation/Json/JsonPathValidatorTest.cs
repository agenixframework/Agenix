using MPP.Core.Validation.Json.Dsl;
using NUnit.Framework;

namespace MPP.Core.NUnitTestProject.Validation.Json
{
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
                .Build()
                .Validate(_payload, Context);
        }

        [Test]
        public void TestValidateMessageElementsWithValidationMatcherSuccessful()
        {
            JsonSupport.Json().JsonPath().Validate()
                .Expression("$.root.element", "@Ignore()@")
                .Expression("$.root.person.name", "@EqualsIgnoreCase('PENNY')@")
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
    }
}