using System.Collections.Generic;
using Agenix.Core.Exceptions;
using Agenix.Core.Validation.Json;
using NUnit.Framework;

namespace Agenix.Core.NUnitTestProject.Validation.Json;

public class JsonElementValidatorTest : AbstractNUnitSetUp
{
    private const bool NotStrict = false;
    private const bool Strict = true;

    private JsonElementValidator _fixture;

    [Test]
    [TestCaseSource(nameof(ValidJsonPairsIfNotStrict))]
    public void ShouldBeValidIfNotStrict(JsonAssertion jsonAssertion)
    {
        var validationItem = ToValidationItem(jsonAssertion);
        _fixture = new JsonElementValidator(NotStrict, Context, new HashSet<string>());
        Assert.DoesNotThrow(() => _fixture.Validate(validationItem));
    }

    [Test]
    [TestCaseSource(nameof(ValidJsonPairsIfNotStrict))]
    public void ShouldBeInvalidIfStrict(JsonAssertion jsonAssertion)
    {
        var validationItem = ToValidationItem(jsonAssertion);
        _fixture = new JsonElementValidator(Strict, Context, new HashSet<string>());
        Assert.Throws<ValidationException>(() => _fixture.Validate(validationItem));
    }

    /// <summary>
    ///     Provides a collection of valid JSON assertion pairs to be used for validation when the strict flag is not set.
    /// </summary>
    /// <returns>
    ///     An IEnumerable of JsonAssertion objects, each containing an actual JSON string and an expected JSON string to
    ///     validate.
    /// </returns>
    public static IEnumerable<JsonAssertion> ValidJsonPairsIfNotStrict()
    {
        return new List<JsonAssertion>
        {
            new("{\"text\":\"Hello World!\", \"index\":5, \"id\":\"x123456789x\"}",
                "{\"id\":\"x123456789x\"}"),

            new(
                "[{\"text\":\"Hello World!\", \"index\":1}, {\"text\":\"Hallo Welt!\", \"index\":2}, {\"text\":\"Hola del mundo!\", \"index\":3}]",
                "[{\"text\":\"Hello World!\", \"index\":1}]"),

            new(
                "[{\"text\":\"Hello World!\", \"index\":1}, {\"text\":\"Hallo Welt!\", \"index\":2}, {\"text\":\"Hola del mundo!\", \"index\":3}]",
                "[{\"text\":\"Hallo Welt!\", \"index\":2}]"),

            new(
                "[{\"text\":\"Hello World!\", \"index\":1}, {\"text\":\"Hallo Welt!\", \"index\":2}, {\"text\":\"Hola del mundo!\", \"index\":3}]",
                "[{\"index\": 1}]"),

            new("[1, 2, 3]", "[2]"),

            new("[1, 2, 1]", "[2, 1]"),

            new("[1, 2, 1]", "[1, 2]")
        };
    }

    [Test]
    [TestCaseSource(nameof(ValidIfStrict))]
    public void ShouldBeValidIfStrict(JsonAssertion jsonAssertion)
    {
        var validationItem = ToValidationItem(jsonAssertion);
        _fixture = new JsonElementValidator(Strict, Context, new HashSet<string>());
        Assert.DoesNotThrow(() => _fixture.Validate(validationItem));
    }

    /// <summary>
    ///     Provides a collection of JSON assertion pairs to be used for validation when the strict flag is set.
    /// </summary>
    /// <returns>
    ///     An array of JsonAssertion objects, each containing an actual JSON string and an expected JSON string for strict
    ///     validation.
    /// </returns>
    public static JsonAssertion[] ValidIfStrict()
    {
        return
        [
            new JsonAssertion(
                "{\"text\":\"Hello World!\", \"index\":5, \"id\":\"x123456789x\"}",
                "{\"text\":\"Hello World!\", \"index\":5, \"id\":\"x123456789x\"}"
            ),
            new JsonAssertion(
                "{\"text\":\"Hello World!\", \"person\":{\"name\":\"John\",\"surname\":\"Doe\"}, \"index\":5, \"id\":\"x123456789x\"}",
                "{\"text\":\"Hello World!\", \"person\":{\"name\":\"John\",\"surname\":\"Doe\"}, \"index\":5, \"id\":\"x123456789x\"}"
            ),
            new JsonAssertion(
                "[{\"text\":\"Hello World!\", \"index\":1}, {\"text\":\"Hallo Welt!\", \"index\":2}, {\"text\":\"Hola del mundo!\", \"index\":3}]",
                "[{\"text\":\"Hello World!\", \"index\":1}, {\"text\":\"Hallo Welt!\", \"index\":2}, {\"text\":\"Hola del mundo!\", \"index\":3}]"
            ),
            new JsonAssertion(
                "[1, {\"text\":\"Hallo Welt!\", \"index\":2}, \"pizza\"]",
                "[1,{\"text\":\"Hallo Welt!\", \"index\":2},\"pizza\"]"
            ),
            new JsonAssertion(
                "{\"greetings\":[{\"text\":\"Hello World!\", \"index\":1}, {\"text\":\"Hallo Welt!\", \"index\":2}, {\"text\":\"Hola del mundo!\", \"index\":3}], \"id\":\"x123456789x\"}",
                "{\"greetings\":[{\"text\":\"Hello World!\", \"index\":1}, {\"text\":\"Hallo Welt!\", \"index\":2}, {\"text\":\"Hola del mundo!\", \"index\":3}], \"id\":\"x123456789x\"}"
            ),
            new JsonAssertion(
                "{\"text\":\"Hello World!\", \"index\":5, \"object\":{\"id\":\"x123456789x\"}, \"greetings\":[{\"text\":\"Hello World!\", \"index\":1}, {\"text\":\"Hallo Welt!\", \"index\":2}, {\"text\":\"Hola del mundo!\", \"index\":3}],}",
                "{\"text\":\"Hello World!\", \"index\":\"@Ignore@\", \"object\":{\"id\":\"@Ignore@\"}, \"greetings\":\"@Ignore@\"}"
            ),
            new JsonAssertion(
                "{\"text\":\"Hello World!\", \"index\":5, \"id\":null}",
                "{\"text\":\"Hello World!\", \"index\":5, \"id\":null}"
            ),
            new JsonAssertion(
                "{}",
                "{}"
            ),
            new JsonAssertion(
                "{\"id\":42}",
                "{\"id\":42}"
            ),
            new JsonAssertion(
                "{\"test\": \"Lorem\"}",
                "{\"test\": \"@EqualsIgnoreCase('lorem')@\"}"
            ),
            new JsonAssertion(
                "[1, 2, 3]",
                "[1, 2, 3]"
            ),
            new JsonAssertion(
                "{ \"books\": [\"book-a\", \"book-b\", \"book-c\"] }",
                "{ \"books\": [\"book-a\", \"book-b\", \"book-c\"] }"
            )
        ];
    }

    /// <summary>
    ///     Provides a collection of invalid JSON assertion pairs to be used for validation in both strict and non-strict
    ///     modes.
    /// </summary>
    /// <returns>
    ///     An array of JsonAssertion objects, each containing an actual JSON string and an expected JSON string that fail
    ///     validation.
    /// </returns>
    public static JsonAssertion[] InvalidJsonPairsOnStrictAndNonStrict()
    {
        return
        [
            new JsonAssertion(
                "{\"myNumbers\": [11, 22, 44]}",
                "{\"myNumbers\": [11, 22, 33]}",
                @"An item in '$[\'myNumbers\']' is missing, expected '33' to be in '[11,22,44]'"
            ),
            new JsonAssertion(
                "{\"text\":\"Hello World!\", \"index\":5, \"id\":\"x123456789x\"}",
                "{\"text\":\"Hello World!\", \"index\":5, \"id\":\"x123456789x\", \"missing\":\"this is missing\"}",
                "Number of entries is not equal in element: '$'",
                "expected '[missing, index, text, id]' but was '[index, text, id]'"
            ),
            new JsonAssertion(
                "{\"greetings\":[{\"text\":\"Hello World!\", \"index\":1}, {\"text\":\"Hallo Welt!\", \"index\":0}, {\"text\":\"Hola del mundo!\", \"index\":3}], \"id\":\"x123456789x\"}",
                "{\"greetings\":[{\"text\":\"Hello World!\", \"index\":1}, {\"text\":\"Hallo Welt!\", \"index\":2}, {\"text\":\"Hola del mundo!\", \"index\":3}], \"id\":\"x123456789x\"}",
                "An item in '$[\\'greetings\\']' is missing, expected '{\"index\":2,\"text\":\"Hallo Welt!\"}' to be in '[{\"index\":1,\"text\":\"Hello World!\"},{\"index\":0,\"text\":\"Hallo Welt!\"},{\"index\":3,\"text\":\"Hola del mundo!\"}]'"
            ),
            new JsonAssertion(
                "{\"numbers\":[101, 42]}",
                "{\"numbers\":[101, 42, 9000]}",
                "Number of entries is not equal in element: '$[\\'numbers\\']'",
                "expected '[101,42,9000]' but was '[101,42]'"
            ),
            new JsonAssertion(
                "{\"test\": \"Lorem\"}",
                "{\"test\": \"@EqualsIgnoreCase('lorem ipsum')@\"}",
                "EqualsIgnoreCaseValidationMatcher failed for field 'test'",
                "Received value is 'Lorem', control value is 'lorem ipsum'"
            ),
            new JsonAssertion(
                "{\"not-test\": \"lorem\"}",
                "{\"test\": \"lorem\"}",
                "Missing JSON entry, expected 'test' to be in '[not-test]'"
            ),
            new JsonAssertion(
                "{\"greetings\":[{\"text\":\"Hello World!\", \"index\":1}, {\"text\":\"Hallo Welt!\", \"index\":2}, {\"text\":\"Hola del mundo!\", \"index\":3}], \"id\":\"x123456789x\"}",
                "{\"greetings\":{\"text\":\"Hello World!\", \"index\":1}, \"id\":\"x123456789x\"}",
                "expected 'JObject'",
                "but was 'JSONArray'"
            ),
            new JsonAssertion(
                "{\"text\":\"Hello World!\", \"index\":5, \"id\":\"x123456789x\"}",
                "{\"text\":\"Hello World!\", \"index\":5, \"id\":null}",
                "expected 'null' but was 'x123456789x'"
            ),
            new JsonAssertion(
                "{\"text\":\"Hello World!\", \"index\":5, \"id\":\"wrong\"}",
                "{\"text\":\"Hello World!\", \"index\":5, \"id\":\"x123456789x\"}",
                "expected 'x123456789x'",
                "but was 'wrong'"
            ),
            new JsonAssertion(
                "{\"text\":\"Hello World!\", \"person\":{\"name\":\"John\",\"surname\":\"wrong\"}, \"index\":5, \"id\":\"x123456789x\"}",
                "{\"text\":\"Hello World!\", \"person\":{\"name\":\"John\",\"surname\":\"Doe\"}, \"index\":5, \"id\":\"x123456789x\"}",
                "expected 'Doe'",
                "but was 'wrong'"
            ),
            new JsonAssertion(
                "{\"greetings\":{\"text\":\"Hello World!\", \"index\":1}, \"id\":\"x123456789x\"}",
                "{\"greetings\":[{\"text\":\"Hello World!\", \"index\":1}, {\"text\":\"Hallo Welt!\", \"index\":2}, {\"text\":\"Hola del mundo!\", \"index\":3}], \"id\":\"x123456789x\"}",
                "expected 'JArray'",
                "but was 'JObject'"
            ),
            new JsonAssertion(
                "{}",
                "{\"text\":\"Hello World!\", \"index\":5, \"id\":\"x123456789x\"}",
                "expected message contents, but received empty message"
            ),
            new JsonAssertion(
                "[1, 2, 2, 1]",
                "[1, 1, 2, 1]"
            ),
            new JsonAssertion(
                "[1]",
                "[1, 1]"
            ),
            new JsonAssertion(
                "[1, 3, 2]",
                "[1, 2, 3]"
            ),
            new JsonAssertion(
                "[3, 2, 1]",
                "[1, 2, 3]"
            )
        ];
    }

    [Test]
    [TestCaseSource(nameof(InvalidJsonPairsOnStrictAndNonStrict))]
    public void ShouldBeInvalidIfNotStrict(JsonAssertion jsonAssertion)
    {
        var validationItem = ToValidationItem(jsonAssertion);
        _fixture = new JsonElementValidator(NotStrict, Context, new HashSet<string>());
        Assert.Throws<ValidationException>(() => _fixture.Validate(validationItem));
    }

    [Test]
    [TestCaseSource(nameof(InvalidJsonPairsOnStrictAndNonStrict))]
    public void ShouldBeInvalid(JsonAssertion jsonAssertion)
    {
        var validationItem = ToValidationItem(jsonAssertion);
        _fixture = new JsonElementValidator(Strict, Context, new HashSet<string>());
        Assert.Throws<ValidationException>(() => _fixture.Validate(validationItem));
    }

    /// <summary>
    ///     Provides a collection of valid JSON assertion pairs that include specific ignore expressions.
    /// </summary>
    /// <returns>
    ///     An array of JsonAssertion objects where certain JSON paths are marked to be ignored during validation.
    /// </returns>
    public static JsonAssertion[] ValidOnlyWithIgnoreExpressions()
    {
        return
        [
            new JsonAssertion(
                "{\"text\":\"Hello World!\", \"index\":5, \"object\":{\"id\":\"x123456789x\"}, \"greetings\":[{\"text\":\"Hello World!\", \"index\":1}, {\"text\":\"Hallo Welt!\", \"index\":2}, {\"text\":\"Hola del mundo!\", \"index\":3}]}",
                "{\"text\":\"Hello World!\", \"index\":\"5\", \"object\":{\"id\":\"?\"}, \"greetings\":\"?\"}",
                new HashSet<string> { "$..index", "$.object.id", "$.greetings" }
            ),
            new JsonAssertion(
                "{\"index\":\"bliblablu\"}",
                "{\"index\":\"tataa\"}",
                new HashSet<string> { "$..index" }
            )
            /*new(
                "{\"index\": {\"anything\": [0]} }",
                "{\"index\": {\"anything\": [55, 66, 77]} }",
                new HashSet<string> { "$.index['anything'][*]" }
            ),
            new(
                "{\"index\": {\"anything\": [0], \"something\": null} }",
                "{\"index\": {\"anything\": [55, 66, 77]} }",
                new HashSet<string> { "$.index['anything'][*]" }
            )*/
        ];
    }

    [TestCaseSource(nameof(ValidOnlyWithIgnoreExpressions))]
    public void ShouldBeValidOnlyWithIgnoreExpressions(JsonAssertion jsonAssertion)
    {
        var validationItem = ToValidationItem(jsonAssertion);
        _fixture = new JsonElementValidator(NotStrict, Context, jsonAssertion.IgnoreExpressions);
        Assert.DoesNotThrow(() => _fixture.Validate(validationItem));
    }

    [TestCaseSource(nameof(ValidOnlyWithIgnoreExpressions))]
    public void ShouldBeInvalidWithoutIgnoreExpressions(JsonAssertion jsonAssertion)
    {
        var validationItem = ToValidationItem(jsonAssertion);
        _fixture = new JsonElementValidator(NotStrict, Context, new HashSet<string>());
        Assert.Throws<ValidationException>(() => _fixture.Validate(validationItem));
    }

    /// <summary>
    ///     Converts a JsonAssertion object into a JsonElementValidatorItem object.
    /// </summary>
    /// <param name="jsonAssertion">The JsonAssertion object containing actual and expected JSON strings to validate.</param>
    /// <returns>
    ///     A JsonElementValidatorItem object initialized with the actual and expected JSON from the provided JsonAssertion.
    /// </returns>
    private static JsonElementValidatorItem<object> ToValidationItem(JsonAssertion jsonAssertion)
    {
        return JsonElementValidatorItem<object>.ParseJson(jsonAssertion.Actual, jsonAssertion.Expected);
    }

    /// <summary>
    ///     Represents an assertion comparison between actual and expected JSON strings.
    /// </summary>
    public record JsonAssertion(
        string Actual,
        string Expected,
        HashSet<string> IgnoreExpressions,
        params string[] MessageContains
    )
    {
        public JsonAssertion(string actual, string expected, params string[] messageContains)
            : this(actual, expected, [], messageContains)
        {
        }
    }
}