using MPP.Core.Validation.Json.Dsl;
using NUnit.Framework;

namespace MPP.Core.NUnitTestProject.Validation.Json
{
    public class JsonPathVariableExtractorTest : AbstractNUnitSetUp
    {
        private readonly string _jsonPayload =
            "{\"text\":\"Hello World!\", \"person\":{\"name\":\"John\",\"surname\":\"Doe\"}, \"index\":5, \"numbers\": [10,20,30,40], \"id\":\"x123456789x\"}";

        [Test]
        public void TestExtractVariables()
        {
            JsonSupport.Json().JsonPath().Extract()
                .Expression("$.KeySet()", "keySet")
                .Expression("$['index']", "index")
                .Expression("$.numbers", "numbers")
                .Expression("$.numbers.Size()", "numbersSize")
                .Expression("$.person", "person")
                .Expression("$.person.name", "personName")
                .Expression("$.ToString()", "toString")
                .Expression("$.Values()", "values")
                .Expression("$.Size()", "size")
                .Expression("$.*", "all")
                .Expression("$", "root")
                .Build()
                .ExtractVariables(_jsonPayload, Context);

            Assert.IsNotNull(Context.GetVariable("keySet"));
            Assert.AreEqual("text, person, index, numbers, id", Context.GetVariable("keySet"));


            Assert.IsNotNull(Context.GetVariable("index"));
            Assert.AreEqual("5", Context.GetVariable("index"));


            Assert.IsNotNull(Context.GetVariable("numbers"));
            Assert.AreEqual("[10,20,30,40]", Context.GetVariable("numbers"));


            Assert.IsNotNull(Context.GetVariable("numbersSize"));
            Assert.AreEqual("4", Context.GetVariable("numbersSize"));

            Assert.IsNotNull(Context.GetVariable("person"));
            Assert.AreEqual("{\"name\":\"John\",\"surname\":\"Doe\"}", Context.GetVariable("person"));

            Assert.IsNotNull(Context.GetVariable("personName"));
            Assert.AreEqual("John", Context.GetVariable("personName"));

            Assert.IsNotNull(Context.GetVariable("toString"));
            Assert.AreEqual(
                "{\"text\":\"Hello World!\",\"person\":{\"name\":\"John\",\"surname\":\"Doe\"},\"index\":5,\"numbers\":[10,20,30,40],\"id\":\"x123456789x\"}",
                Context.GetVariable("toString"));

            Assert.IsNotNull(Context.GetVariable("values"));
            Assert.AreEqual("Hello World!, {\"name\":\"John\",\"surname\":\"Doe\"}, 5, [10,20,30,40], x123456789x",
                Context.GetVariable("values"));

            Assert.IsNotNull(Context.GetVariable("size"));
            Assert.AreEqual("5", Context.GetVariable("size"));

            Assert.IsNotNull(Context.GetVariable("all"));
            Assert.AreEqual("Hello World!, {\"name\":\"John\",\"surname\":\"Doe\"}, 5, [10,20,30,40], x123456789x",
                Context.GetVariable("all"));

            Assert.IsNotNull(Context.GetVariable("root"));
            Assert.AreEqual(
                "{\"text\":\"Hello World!\",\"person\":{\"name\":\"John\",\"surname\":\"Doe\"},\"index\":5,\"numbers\":[10,20,30,40],\"id\":\"x123456789x\"}",
                Context.GetVariable("root"));
        }
    }
}