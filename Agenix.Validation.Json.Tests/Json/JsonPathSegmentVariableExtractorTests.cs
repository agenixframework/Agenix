using Agenix.Api.Exceptions;
using Agenix.Api.Variable;
using Agenix.Validation.Json.Json;
using NUnit.Framework;

namespace Agenix.Validation.Json.Tests.Json;

[TestFixture]
public class JsonPathSegmentVariableExtractorTests : AbstractNUnitSetUp
{
    private const string JsonFixture = "{\"name\": \"Peter\"}";

    private readonly JsonPathSegmentVariableExtractor _unitUnderTest = new();

    [Test]
    public void TestExtractFromJson()
    {
        var jsonPath = "$.name";
        var matcher = MatchSegmentExpressionMatcher(jsonPath);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(_unitUnderTest.CanExtract(Context, JsonFixture, matcher), Is.True);
            Assert.That(_unitUnderTest.ExtractValue(Context, JsonFixture, matcher), Is.EqualTo("Peter"));
        }
    }

    [Test]
    public void TestExtractFromNonJsonPathExpression()
    {
        var json = "{\"name\": \"Peter\"}";

        var nonJsonPath = "name";
        var matcher = MatchSegmentExpressionMatcher(nonJsonPath);

        Assert.That(_unitUnderTest.CanExtract(Context, json, matcher), Is.False);
    }

    [Test]
    public void TestExtractFromJsonExpressionFailure()
    {
        var json = "{\"name\": \"Peter\"}";

        var invalidJsonPath = "$.$$$name";
        var matcher = MatchSegmentExpressionMatcher(invalidJsonPath);

        Assert.That(_unitUnderTest.CanExtract(Context, json, matcher), Is.True);

        // NUnit way to assert exceptions
        Assert.Throws<AgenixSystemException>(() =>
            _unitUnderTest.ExtractValue(Context, json, matcher));
    }

    /// <summary>
    ///     Create a variable expression jsonPath matcher and match the first jsonPath
    /// </summary>
    /// <param name="jsonPath">The JSON path to match</param>
    /// <returns>A matcher that has found its first match</returns>
    private VariableExpressionSegmentMatcher MatchSegmentExpressionMatcher(string jsonPath)
    {
        var variableExpression = $"jsonPath({jsonPath})";
        var matcher = new VariableExpressionSegmentMatcher(variableExpression);
        Assert.That(matcher.NextMatch(), Is.True);
        return matcher;
    }
}
