using Agenix.Validation.Json.Validation;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace Agenix.Validation.Json.Tests.Json;

public class JsonElementValidatorItemTest
{
    public static IEnumerable<TestCaseData> GetPathPairs()
    {
        yield return new TestCaseData(
            "$['propertyA']",
            new JsonElementValidatorItem<string>("propertyA", "", "")
        );

        yield return new TestCaseData(
            "$['propertyA']",
            new JsonElementValidatorItem<string>("propertyA", "", "")
                .Parent(new JsonElementValidatorItem<string>(null, "", ""))
        );

        yield return new TestCaseData(
            "$['propertyA']['propertyB']",
            new JsonElementValidatorItem<string>("propertyB", "", "")
                .Parent(new JsonElementValidatorItem<string>("propertyA", "", "")
                    .Parent(new JsonElementValidatorItem<string>(null, "", "")))
        );

        yield return new TestCaseData(
            "$['propertyA'][1]",
            new JsonElementValidatorItem<string>(1, "", "")
                .Parent(new JsonElementValidatorItem<string>("propertyA", "", "")
                    .Parent(new JsonElementValidatorItem<string>(null, "", "")))
        );

        yield return new TestCaseData(
            "$[1]",
            new JsonElementValidatorItem<string>(1, "", "")
                .Parent(new JsonElementValidatorItem<string>(null, "", ""))
        );
    }

    [Test]
    [TestCaseSource(nameof(GetPathPairs))]
    public void ShouldGetJsonPath(string expectedPath, JsonElementValidatorItem<string> fixture)
    {
        Assert.That(fixture.GetJsonPath(), Is.EqualTo(expectedPath));
    }

    public static IEnumerable<TestCaseData> GetNamePairs()
    {
        yield return new TestCaseData(
            "$",
            new JsonElementValidatorItem<string>(null, "", "")
        );

        yield return new TestCaseData(
            "propertyA",
            new JsonElementValidatorItem<string>("propertyA", "", "")
        );

        yield return new TestCaseData(
            "[2]",
            new JsonElementValidatorItem<string>(2, "", "")
        );
    }

    [Test]
    [TestCaseSource(nameof(GetNamePairs))]
    public void ShouldGetName(string expectedName, JsonElementValidatorItem<string> fixture)
    {
        Assert.That(fixture.GetName(), Is.EqualTo(expectedName));
    }
}
